# Overview
This application is written in c# using Cloud Functions using HTTP-triggered function.
## Requirements
1.Google Cloud console with billing enabled
	- Make sure that billing is enabled for your Cloud project. Learn how to confirm that billing is enabled for [your project.](https://cloud.google.com/billing/docs/how-to/modify-project#confirm_billing_is_enabled_on_a_project)

2.have installed the Cloud SDK on your local machine, or in Cloud Shell, to run the following commands 
	- follow the instruction [here](https://cloud.google.com/sdk/docs/install)
	- Install and initialize the [Cloud SDK.](https://cloud.google.com/sdk/docs)

3.Firebase CLI
	- follow the instruction [here](https://firebase.google.com/docs/cli)

4.Node.js
	- [Install Node.js](https://nodejs.org/en/)

5.Npm
	- [Getting started](https://docs.npmjs.com/getting-started)
	- [npm CLI](https://docs.npmjs.com/cli/v8/commands/npm-install)

6.Angular
	- [Getting started](https://angular.io/start) and install using command
	
	`npm install -g @angular/cli`

**Note**: _Need a command prompt? You can use the Google Cloud Shell. The Google Cloud Shell is a command line environment that already includes the Cloud SDK, so you don't need to install it. The Cloud SDK also comes preinstalled on Google Compute Engine Virtual Machines._ [Open Console](https://console.cloud.google.com/?cloudshell=true)


## Create a Google Cloud project

_Update components_

`gcloud components update`

_Install aplha component for firestore_

`gcloud components install alpha`

_Install beta component for firestore_

`gcloud components install beta`

_Create project_

`gcloud projects create game-project-ahfp6 --name="Game Project ahfp6"` 

_Be sure your gcloud tool use new project_

`gcloud config set project game-project-ahfp6`
	
_Check if the project was created_

`gcloud projects describe game-project-ahfp6`

### Enable billing for project

_List your billing accounts_

`gcloud alpha billing accounts list`

_From the the list of billing accounts choose ACCOUNT_ID_

`gcloud beta billing projects link game-project-ahfp6 --billing-account=0X0X0X-0X0X0X-0X0X0X`


### Enable cloud build service

`gcloud services enable cloudfunctions.googleapis.com`

`gcloud services enable iamcredentials.googleapis.com`

`gcloud services enable cloudbuild.googleapis.com`

`gcloud services enable appengine.googleapis.com`

### Add firestore database

_Create database firestore native ( require Name: gcloud Alpha Commands)_

`gcloud app create --region=europe-central2`

_Create native database for app_

`gcloud alpha firestore databases create --project game-project-ahfp6 --region=europe-central2`

### Create bucket for profile

_Create a bucket to store images (before check you have billing enabled)_
_for further information check https://cloud.google.com/billing/docs/how-to/modify-project_

`gsutil mb -b on -l us-east1 gs://profilebucket-ahfp6/`

_Run command to make all objects in a bucket readable to everyone on the public internet_

`gsutil iam ch allUsers:objectViewer gs://profilebucket-ahfp6`

## Create firebase project

_Authenticate to your Firebase account. Requires access to a web browser (this take few minutes)_

`firebase login --no-localhost`

_Adding Firebase resources to Google Cloud Platform project_

`firebase projects:addfirebase game-project-ahfp6`

## Create backend

`mkdir game-project-ahfp6`

`cd game-project-ahfp6`

_Clone git repository_

`git clone https://github.com/partenziF/AfterhoursGameTest`


## Create service account

_Create the service account_

`gcloud iam service-accounts create serviceaccount`

_Grant roles to the service account_

`gcloud projects add-iam-policy-binding game-project-ahfp6 --member="serviceAccount:serviceaccount@game-project-ahfp6.iam.gserviceaccount.com" --role=roles/owner`


_Generate the key file, replace GoogleApplicationCredentials.json in AfterhoursGameTest\AfterhoursGameTest_

`cd AfterhoursGameTest\AfterhoursGameTest`

`gcloud iam service-accounts keys create GoogleApplicationCredentials.json --iam-account=serviceaccount@game-project-ahfp6.iam.gserviceaccount.com`


### Update configuration file appsettings.json

Find file named appsettings.json open it and replace in GeneralConfiguration section the value for firebase and BucketName.

_"Firestore": "game-project-ahfp6"_

_"BucketName": "profilebucket-ahfp6"_

_"GoogleApplicationCredentials": "GoogleApplicationCredentials.json"_

_**Note:** Deploy function works on game-project-ahfp6\AfterhoursGameTest directory_

`cd game-project-ahfp6\AfterhoursGameTest\`

_Deploy Google Cloud Function_

`gcloud functions deploy login --runtime dotnet3 --trigger-http --allow-unauthenticated --entry-point=login.Login --set-build-env-vars=GOOGLE_BUILDABLE=login`

`gcloud functions deploy userinfo --runtime dotnet3 --trigger-http --allow-unauthenticated --entry-point=userinfo.UserInfo --set-build-env-vars=GOOGLE_BUILDABLE=userinfo`

`gcloud functions deploy registeruser --runtime dotnet3 --trigger-http --allow-unauthenticated --entry-point=registeruser.RegisterUser --set-build-env-vars=GOOGLE_BUILDABLE=registeruser`

`gcloud functions deploy uploadprofile --runtime dotnet3 --trigger-http --allow-unauthenticated --entry-point=uploadprofile.UploadProfile --set-build-env-vars=GOOGLE_BUILDABLE=uploadprofile`

`gcloud functions deploy getprofile --runtime dotnet3 --trigger-http --allow-unauthenticated --entry-point=getprofile.GetProfile --set-build-env-vars=GOOGLE_BUILDABLE=getprofile`

`gcloud functions deploy users --runtime dotnet3 --trigger-http --allow-unauthenticated --entry-point=users.Users --set-build-env-vars=GOOGLE_BUILDABLE=users`

_**Note:** Usefull in case of error you can viewing logs of function with command_

`gcloud functions logs read <FUNCTION-NAME>`

## Create Angular App

_In  game-project-ahfp6 directory clone repository contains angular application_

`cd game-project-ahfp6`

_Clone git repository_

`git clone https://github.com/partenziF/afterhoursApp.git`

`cd afterhoursApp/`

_Use project created_

`firebase use game-project-ahfp6`

_Check your project list_

`firebase projects:list`

_Now project you should see (current) near project id_

_Create new app for selected project_

`firebase apps:create WEB "Game Project ahfp6"`

## Enable anonymous auth:

In the [Firebase console](https://console.firebase.google.com/), open the Auth section.


On the Sign-in Methods page, enable the Anonymous sign-in method.

_Now to get the configuration file type, anche if request choose the app_

`firebase apps:sdkconfig WEB` 

_replace the code in initializeApp in afterhoursApp/src/environments/environments.ts and srcenvironments/environments.prod.ts_

_When the function finishes deploying, take note of the httpsTrigger.url property or find it using the following command:_

_It should look like this:https://GCP_REGION-PROJECT_ID.cloudfunctions.net/<FUNCTION-NAME>_

`gcloud functions describe login`


_replace the httpsTrigger.url property value within afterhoursApp/src/environments/environments.ts and /src/environments/environments.prod.ts in the baseURL structure_

`gcloud functions describe userinfo`

`gcloud functions describe registeruser`

`gcloud functions describe uploadprofile`

`gcloud functions describe getprofile`

`gcloud functions describe users`

_Authenticate to your Firebase account. Requires access to a web browser_

`firebase login --no-localhost`

_Build application with angular_ 

`npm install`

`ng build`

_Deploy app on firebase hosting_

`firebase target:apply hosting afterhoursApp game-project-ahfp6`

`firebase deploy`


## Finish 

Open browser and test application!