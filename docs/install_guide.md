# Installation

This guide describes how to install Data Lineage Services and configure a data lineage project.

## Contents

- [MSI Installer](#msi-installer)
  - [Folder Structure](#folder-structure)
- [Configuration App](#configuration-app)
  - [Connecting to DB](#connecting-to-db)
  - [Registry Entries](#registry-entries)
  - [Service Configuration](#service-configuration)
- [Create Project](#create-project)
  - [SQL Databases](#sql-databases)
  - [SSIS](#ssis)
  - [SSAS](#ssas)
  - [SSRS](#ssrs)
  - [Power BI](#power-bi)
  - [Test the Configuration](#test-the-configuration)
- [Next Steps](#next-steps)

## MSI Installer

You can download the installer from the latest release at [https://github.com/rjankovic/DLS/releases](https://github.com/rjankovic/DLS/releases).  
  
![image](https://user-images.githubusercontent.com/2221666/167270821-3f4164c8-827e-42d4-87b5-1eb2253b0089.png)

The installer will require elevated permissions to extract the files needed. After the files have been copied, the [configuration app](#configuration-app) will launch.

### Folder Structure
The default folder structure after installation is as follows:  
![image](https://user-images.githubusercontent.com/2221666/167270789-68862a86-b044-4c30-9bff-19870ec01776.png)
 - **client** contains the main client application (DLS Manager). Here is where you can configure projects and explore data lineage. The installer also created a shortcut to this client on the desktop.
 - **configuration** contains the configuration application that was launched at the end of the installation. If the installation fails or if you need to reconfigure, you can run it later at any point.
 - **extractor** is a console app for extracting the metadata of databases, reports, etc.. It saves these metadata in text files, which are then processed by the DLS service. Also, a config subfolder will be created in the extractor folder, which stores the credentials for connecting to the 
 - **service** is the backend of the app - it parses the metadata and provides lineage info to client apps. It can run as a Windows service or as a background process within the client or as an independent Windows service. (see [Service Configuration](#service-configuration)).

## Configuration App

The configuration app initializes the lineage DB structures, configures registry entries and configures the backend service. It's located in the configuration subfolder of the install directory.

### Connecting to DB

The first step is to create a new database in your SQL Server instance (you can use the Developer edition of SQL Server if you want). Fill in the server and DB name and click Connect. You can use any DB name. In the ideal case, the connection should succeed:

![image](https://user-images.githubusercontent.com/2221666/167289105-a4038f40-de3b-4245-af77-cc756a9b2536.png)

When running the MSI installer, the app gets launched with a system account, which could have issues connecting to the DB:

![image](https://user-images.githubusercontent.com/2221666/167289787-411f27aa-a18f-47f3-b3f1-4b4b749c11fd.png)

In this case, you can close the config app and the setup and run the config app again from the **configuration** folder using the admin account, which should have sufficient permissions. Or you can add permissions to the system account to access the DLS DB.
  
After the connection succeeds, you can just click Configure DLS to finish the configureation using the default settings, or you can choose to install the backend as a Windows service (see [Service Configuration](#service-configuration)).

### Registry Entries

The configuration app also sets a few registry entries in **HKLM\HKEY_LOCAL_MACHINE\SOFTWARE\DLS**. These include
 - **CustomerDatabaseConnectionString** Connection string to the DLS database.
 - **ExtractorPath** Path to the metadata extractor in the extractor subdir of the install dir.
 - **ServiceRunsInConsole** True if the backend service runs as a backend process within the client app; False if it runs as a Windows service.
 - **UploaderConnectionString** Not used now, later this will be used to separate the metadata extract and processing.

### Service Configuration

By default, the servie runs in background within the client. However, you may want to use the same instance of the service instance for all the users. In that case, you can install the DLS Windows service. Then the configuration app will ask for the credentials which the service should use:

![image](https://user-images.githubusercontent.com/2221666/167292105-aa14b71f-3303-4b1b-b980-784211de4f73.png)

## Create Project

The project is configured from the client application "DLS.Manager.exe", the Project tab in the top menu.   
Add a project, pick a name, then click Open and select the project. Now click Configure to set the connections to the components of your solution, whoose data flow you want to analyse.  

![image](https://user-images.githubusercontent.com/2221666/167292860-d0a5d3ba-2a7f-4a7d-9e7a-b4bebee7dc32.png)  

after you have configured the components, click Save. If you want to add / change some components, you can do that at any time. The next time you refresh the lineage data, the metadata model will be cleared and rebuilt from scratch.  

On each tab of the project configuration, you can Add new items, Edit the selected row, or Remove it.

### SQL Databases
For SQL databases, simply set the server and DB name, and enter the credentials as needed. The account that is used to extract metadata needs the permissions to extract the DB definitions. If you use Windows authentication and refresh the lineage from the client app, the credentials of the user running the DLS Manager will be used.

### SSIS
SSIS projects are extracted from the SSIS catalog. Simply select the server name, click Connect and select the project.

### SSAS
Simply select the server name, click Connect and select the SSAS database. DLS can process both tabular and multidimensional models.

### SSRS
You can either connect to SSRS in native mode (Report Server) or in SharePoint. Enter the report server root path (e.g. **http://localhost/reportserver**) or the sharepoint site URL for SharePoint mode. You can then also further specify the folder containing the reports to be processed. All the subfolders of the selected folder will be traversed and processed.

### Power BI
The easiest option here is to have the reports stored in a folder where they can be extract from. If this is the case, just select Disk Folder and locate the folder. All the subfolders of this folder will be searched for .pbix files to process.  
  
The other option is to connect to the Power BI Service. Here, the Power BI REST API will be used to extract the report definitions. For that, you need to register an app in Azure with access to the REST API. You can use hte app registration tool here: [https://dev.powerbi.com/apps](https://dev.powerbi.com/apps). Then use the app ID in the configuration and enter the neccessary credentials. If you want to connect to a specific Workspace, you can find the workspace ID in the URL - app.powerbi.com/groups/**WorkspaceID**/.  
  
Finally, you can also connect to the Report Server (similarly to SSRS) and extract Power BI Files from there.  
### Test the configuration
After you have finished and saved the configuration, you should test the metadata extract. Go to the Lineage panel and click Updte. After confirmation, the extractor window should appear:  
![image](https://user-images.githubusercontent.com/2221666/167294542-3071cf3e-85a6-4fde-a0fd-34c30bb4c981.png)  
This extracts the metadata to a folder in AppData\Local\Temp\DLS_extract. If the extractor finishes in good order, you have configured the project correctly. If there are some issues and it looks like the app's fault, please submit and issue to https://github.com/rjankovic/DLS/issues

If the extract goes well, you will see the backend activity in the log window. In the end, a message will be displayed confirming that the lineage model has been updated:

![image](https://user-images.githubusercontent.com/2221666/167295094-53d5833b-0502-46ac-8bf7-472f27de87ab.png)

## Next Steps

**[User guide](user_guide)**  
**[Report an issue](https://github.com/rjankovic/DLS/issues)**






