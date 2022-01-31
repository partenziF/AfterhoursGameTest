#Overview
This application is written in c# using Cloud Functions using HTTP-triggered function.
##Requirements
1.Google Cloud console with billing enabled
	Make sure that billing is enabled for your Cloud project. Learn how to confirm that billing is enabled for your project. https://cloud.google.com/billing/docs/how-to/modify-project#confirm_billing_is_enabled_on_a_project

2.have installed the Cloud SDK on your local machine, or in Cloud Shell, to run the following commands 
	follow the instruction in https://cloud.google.com/sdk/docs/install	
	nstall and initialize the Cloud SDK. https://cloud.google.com/sdk/docs
3.Firebase CLI
follow the instruction in https://firebase.google.com/docs/cli

**Note**: _Need a command prompt? You can use the Google Cloud Shell. The Google Cloud Shell is a command line environment that already includes the Cloud SDK, so you don't need to install it. The Cloud SDK also comes preinstalled on Google Compute Engine Virtual Machines._ https://console.cloud.google.com/?cloudshell=true


## Create a Google Cloud project

_Update components_

**gcloud components update**

_Install aplha component for firestore_

**gcloud components install alpha**

_Install beta component for firestore_

**gcloud components install beta**

_Create project_

**gcloud projects create afterhours-test-federica --name="Afterhours Test Federica""** 

_Be sure your gcloud tool use new project_

**gcloud config set project afterhours-test-federica**
	
_Check if the project was created_

**gcloud projects describe afterhours-test-federica**

_Create database firestore native ( require Name: gcloud Alpha Commands)_

**gcloud app create --region=europe-central2**

_Create native database for app_
**gcloud alpha firestore databases create --project afterhours-test-federica --region=europe-central2**
**_Would you like to enable and retry (this will take a few minutes)? (y/N)? y_**

_Create a bucket to store images (before check you have billing enabled)_
_for further information check https://cloud.google.com/billing/docs/how-to/modify-project_

_List your billing accounts_
gcloud alpha billing accounts list

_From the the list of billing accounts choose ACCOUNT_ID

**gcloud beta billing projects link my-project --billing-account=0X0X0X-0X0X0X-0X0X0X**


**gsutil mb -b on -l us-east1 gs://afterhoursgame/**

_Authenticate to your Firebase account. Requires access to a web browser_

**firebase login --no-localhost**

##Create firebase project

_Create new firebase project_

**firebase projects:addfirebase afterhours-test-federica**

_Use project created_
firebase use afterhours-test-federica

_Check your project list_

**firebase projects:list**

_Now project you should see (current) near project id_
_Create new app for selected project_

**firebase apps:create WEB "Afterhours Firebase"**

_Now to get the configuration file type, anche if request choose the app_
**firebase apps:sdkconfig WEB** 

_replace the code in initializeApp in /src/environment/environments.ts and /src/environment/environments.prod.ts_

_Authenticate to your Firebase account. Requires access to a web browser_

**firebase login**

_Deploy app on firebase hosting_

**firebase deploy**

##Create file

**mkdir afterhours-test-federica**
**cd afterhours-test-federica**

_Clone git repository_
**git clone https://github.com/partenziF/AfterhoursGameTest.git**
**cd AfterhoursGameTest/**

_Enable cloud build service_
	**gcloud services enable cloudbuild.googleapis.com**


_Deploy Google Cloud Function_
gcloud functions deploy login --runtime dotnet3 --trigger-http --allow-unauthenticated --entry-point=login.Login --set-build-env-vars=GOOGLE_BUILDABLE=login

_When the function finishes deploying, take note of the httpsTrigger.url property or find it using the following command:_

**gcloud functions describe my-first-function**

_It should look like this:https://GCP_REGION-PROJECT_ID.cloudfunctions.net/<FUNCTION-NAME>_




gcloud functions deploy userinfo --runtime dotnet3 --trigger-http --allow-unauthenticated --entry-point=userinfo.UserInfo --set-build-env-vars=GOOGLE_BUILDABLE=userinfo
gcloud functions deploy registeruser --runtime dotnet3 --trigger-http --allow-unauthenticated --entry-point=registeruser.RegisterUser --set-build-env-vars=GOOGLE_BUILDABLE=registeruser

gcloud functions deploy uploadprofile --runtime dotnet3 --trigger-http --allow-unauthenticated --entry-point=uploadprofile.UploadProfile --set-build-env-vars=GOOGLE_BUILDABLE=uploadprofile
gcloud functions deploy getprofile --runtime dotnet3 --trigger-http --allow-unauthenticated --entry-point=getprofile.GetProfile --set-build-env-vars=GOOGLE_BUILDABLE=getprofile

gcloud functions deploy users --runtime dotnet3 --trigger-http --allow-unauthenticated --entry-point=users.Users --set-build-env-vars=GOOGLE_BUILDABLE=users

_Viewing logs_
gcloud functions logs read <FUNCTION-NAME>