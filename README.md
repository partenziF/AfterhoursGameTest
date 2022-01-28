#Overview
This application is written in c# using Cloud Functions using HTTP-triggered function.
##Requirements
1.Google Cloud console with billing enabled
2.have installed the Cloud SDK on your local machine, or in Cloud Shell, to run the following commands 
follow the instruction in https://cloud.google.com/sdk/docs/install
3.Firebase CLI
follow the instruction in https://firebase.google.com/docs/cli

## Create a Google Cloud project

_Update components_

**gcloud components update**

_Install aplha component for firestore_

**gcloud components install alpha**

_Create project_

**gcloud projects create afterhours-test-federica --name="Afterhours Test Federica** 

_Be sure your gcloud tool use new project_

**gcloud config set project afterhours-test-federica**
	
_Check if the project was created_

**gcloud projects describe afterhours-test-federica**

_Create database firestore native ( require Name: gcloud Alpha Commands)_

**gcloud app create --region=europe-central2**

**gcloud alpha firestore databases create --project afterhours-test-federica --region=europe-central2**

_Create a bucket to store images (before check you have billing enabled)_

**gsutil mb -b on -l us-east1 gs://afterhoursgame/**

_Authenticate to your Firebase account. Requires access to a web browser_

**firebase login**

##Creaate firebase project

_Create new project_

**firebase projects:create**

_Please specify a unique project id enter afterhours-test-federica_
_What would you like to call your project? Afterhours Test Federica


##Create firebase app

_Use project created_
firebase use afterhours-test-federica


_Check your project list_

**firebase projects:list**

_Now project afterhours-test-federica should be (current) near project id_
_Create new app for selected project_

**firebase apps:create**

_Please choose the platform of the app: Web_
_What would you like to call your app? () Afterhours Test Federica

_Now to get the configuration file type, anche if request choose the app_
**firebase apps:sdkconfig WEB** 

_replace the code in initializeApp in /src/environment/environments.ts and /src/environment/environments.prod.ts_

_Authenticate to your Firebase account. Requires access to a web browser_

**firebase login**

_Deploy app on firebase hosting_

**firebase deploy**

