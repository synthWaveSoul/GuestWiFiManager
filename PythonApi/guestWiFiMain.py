from configFile import *
from flask import Flask, request
from flask_restful import Resource, Api
import secrets
import string
from datetime import date, timedelta, datetime
import meraki
from cryptography.fernet import Fernet
import pyodbc

# connect to Meraki platform
meraki_dashboard = meraki.DashboardAPI(API_KEY, output_log=False)

app = Flask("GuestWiFiManager")
api = Api(app)

# used for encrypting and decrypting password
fernet = Fernet(encryption_key)

# generate random password for wifi access
def generate_password():
    alphabet = string.ascii_letters + string.digits
    pwd_length = 8
    pwd = ""
    for i in range(pwd_length):
        pwd += ''.join(secrets.choice(alphabet))
    return pwd

# based on the date received (expiry date of the account from Meraki platform),
# checks whether the email ID is currently in use
def is_active(long_date):
    
    # check and return is it still active
    if datetime.now() > datetime.strptime(correct_date_format(long_date), '%Y-%m-%d %H:%M:%S'):
        # is no longer active
        return "no"
    else:
        # is active
        return "yes"

# decrypt password from db
# password stored in the database is encrypted with the key
def decrypt_password(pwd_from_db):

    # lenght of encrypted string is over 40 characters
    # need to check if string in db is shorter (for newly created users it is shorter)
    if len(pwd_from_db) < 15:
        return "not set"
    else:
        return fernet.decrypt(bytes(pwd_from_db, 'utf-8')).decode()

# correct date format from long one (2023-09-27T22:59:00.000000Z, it is Meraki date format) to short (2023-09-27 22:59:00)
def correct_date_format(long_date_format):
    short_date_format = long_date_format[:19].replace("T", " ")
    return short_date_format

# class checks for the currently logged in user: full name of the user, details of the history for all accounts of the logged user, how many available accounts has left
# triggered at the start of the application and each time access is granted and revoked
class CheckUserNameHistoryDetailsAvailability(Resource):

    def get(self, user_id):

        # get full name of the user so "Hello 'name'" can be displayed in the app
        query_get_full_name = "SELECT * FROM notRealNameOfUserDetailsTable WHERE userId = ?"
        try:
            conn = pyodbc.connect(sql_connection_string)
            cursor = conn.cursor()
            cursor.execute(query_get_full_name, user_id)
            result_query_get_full_name = cursor.fetchall()
            result_to_json = []
            for row in result_query_get_full_name:
                result_to_json.append({"userId": row[0], "user_dc_name": row[1], "user_full_name": row[2]})
            logged_user = {
                "userId": result_to_json[0]["userId"],
                "user_dc_name": result_to_json[0]["user_dc_name"],
                "user_full_name": result_to_json[0]["user_full_name"]
            }
        except pyodbc.Error as e:
            return {"error": str(e)}
        conn.close()
        
        # get Meraki email ID list for current user
        # each email stored on the Meraki platform has an ID assigned to it
        # while working with Meraki API, email ID needs to be used, not the name
        query_get_meraki_id = "SELECT merakiEmailId FROM notRealNameOfDetailsTable WHERE userId = ?"
        try:
            conn = pyodbc.connect(sql_connection_string)
            cursor = conn.cursor()
            cursor.execute(query_get_meraki_id, user_id)
            result_query_get_meraki_id = cursor.fetchall()

            # convert SQL results to an array of strings
            list_of_meraki_email_id = []
            for row in result_query_get_meraki_id:
                list_of_meraki_email_id.append(row[0])
            
        except pyodbc.Error as e:
            return {"error": str(e)}
        conn.close()

        # check how many accounts (merakiEmailId) the user can use (for new guests)
        #
        # ////////// STEP 1 - for every merakiEmailId that user have, update isActive column in DB ////////// 
        for id in list_of_meraki_email_id:
            try:
                # check on Meraki platform expiration date for the merakiEmailId
                # it is possible to change it manually on Meraki platform, in that case date in DB won't be updated
                user_details_response = meraki_dashboard.networks.getNetworkMerakiAuthUser(network_id, id)
                expire_date_from_meraki = user_details_response["authorizations"][0]["expiresAt"]
                active_status = is_active(expire_date_from_meraki)
                expire_date_short_correct = correct_date_format(expire_date_from_meraki)

                query_update_isactive = "UPDATE notRealNameOfDetailsTable SET isActive = ?, expires = ? WHERE merakiEmailId = ?"
                try:
                    conn = pyodbc.connect(sql_connection_string)
                    cursor = conn.cursor()
                    cursor.execute(query_update_isactive, active_status, expire_date_short_correct, id)
                    conn.commit()
                except pyodbc.Error as e:
                    return {"error": str(e)}
                conn.close()
            except Exception as e:
                return {"error": str(e)}
        #  
        # ////////// STEP 2 - count all "no" in isActive column in DB ////////// 
        query_count_available_accounts = "SELECT count(isActive) FROM notRealNameOfDetailsTable WHERE userId = ? AND isActive = 'no'"
        try:
            conn = pyodbc.connect(sql_connection_string)
            cursor = conn.cursor()
            cursor.execute(query_count_available_accounts, user_id)
            quantity = cursor.fetchone()[0]
        except pyodbc.Error as e:
            return {"error": str(e)}
        conn.close()

        # set accounts history details to the app
        # it is visible in details tab in the app
        query_select_data_for_app = "SELECT name, merakiEmailId, merakiEmailLogin, isActive, expires, createdAt, password FROM notRealNameOfDetailsTable WHERE userId = ?"
        try:
            conn = pyodbc.connect(sql_connection_string)
            cursor = conn.cursor()
            cursor.execute(query_select_data_for_app, user_id)
            result_query_select_data_for_app = cursor.fetchall()
            accounts_history_details = []
            for row in result_query_select_data_for_app:
                accounts_history_details.append({"name": row[0], "merakiEmailId": row[1], "merakiEmailLogin": row[2], "isActive": row[3], "expires": row[4], "createdAt": row[5], "password": decrypt_password(row[6])})
        except pyodbc.Error as e:
            return {"error": str(e)}
        conn.close()

        return {"error": "no_error", "quantity": quantity, "fullName": logged_user["user_full_name"], "accountsHistoryDetails": accounts_history_details}
    
# get first unactive account (merakiEmailId) from DB and set up Wi-Fi access on Meraki platform for it
# if successfull, update local DB so it stores the same information like Meraki platform
# triggered after press button "Set access" in app
class SetAccess(Resource):

    def get(self, given_name, given_duration, given_user_id):

        # prepare expire date for Meraki platform in Meraki's date format
        expire_date = (date.today() + timedelta(days=int(given_duration))).strftime('%Y-%m-%d') + "T23:59:00.000000Z"

        # generate random password for Wi-Fi access
        password = generate_password()

        # get first unactive account (merakiEmailId) to use for the logged in user
        query_select_one_free_account = "SELECT TOP 1 merakiEmailId, merakiEmailLogin FROM notRealNameOfDetailsTable WHERE userId = ? AND isActive = 'no'"
        try:
            conn = pyodbc.connect(sql_connection_string)
            cursor = conn.cursor()
            cursor.execute(query_select_one_free_account, given_user_id)
            meraki_email_id = cursor.fetchone()[0]
            cursor.execute(query_select_one_free_account, given_user_id)
            meraki_email_login = cursor.fetchone()[1]
        except pyodbc.Error as e:
            return {"error": str(e)}
        conn.close()

        try:
            # set up Wi-Fi access on Meraki platform for above merakiEmailId (main goal of the app)
            response_update_meraki = meraki_dashboard.networks.updateNetworkMerakiAuthUser(
                network_id, meraki_email_id, 
                name=given_name, 
                password=password, 
                emailPasswordToUser=False, 
                authorizations=[{'ssidNumber': 1, 'expiresAt': expire_date}]
                )
        except Exception as e:
            return {"error": str(e)}
        else:
            # ///// if update on Meraki platform was successful, then update the DB

            # encrypt thepassword to be stored in the DB
            # variable is in bytes so it needs to be converted to string later
            enc_password = fernet.encrypt(password.encode())

            createdAt = (datetime.now()).strftime("%Y-%m-%d %H:%M:%S")
            expire_date_short = correct_date_format(expire_date)
            query_update_localdb_after_meraki_update = f"UPDATE notRealNameOfDetailsTable SET name=?, isActive='yes', expires=?, createdAt=?, password='{enc_password.decode()}' WHERE merakiEmailId=?"
            try:
                conn = pyodbc.connect(sql_connection_string)
                cursor = conn.cursor()
                cursor.execute(query_update_localdb_after_meraki_update, given_name, expire_date_short, createdAt, meraki_email_id)
                conn.commit()
            except pyodbc.Error as e:
                return {"error": str(e)}
            conn.close()
        
        return {"error": "no_error", "merakiEmail": meraki_email_login, "password": password, "expire": expire_date_short}

# revoke access for selected account (merakiEmailId)
class RevokeAccess(Resource):

    def get(self, delete_meraki_email_id):

        # response message for the app
        # hardcoded "unknown" helps to handle response in the app in case of error
        response = "unknown"

        # prepare expire date for Meraki platform, subtract 1 minute from current time and convert string to Meraki's date format
        expire_date_for_meraki = (str(datetime.now() - timedelta(minutes=1)).replace(" ", "T")) + "Z"

        try:
            # update given account (delete_meraki_email_id) on Meraki platform to expire
            response_update_meraki = meraki_dashboard.networks.updateNetworkMerakiAuthUser(
                network_id, delete_meraki_email_id, 
                emailPasswordToUser=False, 
                authorizations=[{'ssidNumber': 1, 'expiresAt': expire_date_for_meraki}]
                )
            response = "updated only on Meraki"
        except Exception as e:
            return {"error": str(e)}
        else:
            # if update on meraki was successful, update local DB
            expire_date_short = correct_date_format(expire_date_for_meraki)
            query_update_localdb_after_meraki_update = "UPDATE notRealNameOfDetailsTable SET isActive='no', expires=? WHERE merakiEmailId=?"
            try:
                conn = pyodbc.connect(sql_connection_string)
                cursor = conn.cursor()
                cursor.execute(query_update_localdb_after_meraki_update, expire_date_short, delete_meraki_email_id)
                conn.commit()
                response = "Successful update"
            except pyodbc.Error as e:
                return {"error": str(e)}
            conn.close()

        return {"error": "no_error","response": response}

api.add_resource(CheckUserNameHistoryDetailsAvailability, '/pythonapi/get/<user_id>')
api.add_resource(SetAccess, '/pythonapi/put/name=<given_name>&duration=<given_duration>&userid=<given_user_id>')
api.add_resource(RevokeAccess, '/pythonapi/delete/<delete_meraki_email_id>')

if __name__ == '__main__':
    app.run(host="localhost", port=8000)
