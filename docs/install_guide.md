# DLS Installation

This guide describes how to install Data Lineage Services and configure a data lineage project.

## Contents

- [MSI Installer](#msi-installer)
  - [Folder Structure](#folder-structure)
- [Configuration App](#configuration-app)
  - [Connecting to DB](#connecting-to-db)
  - [Registry Entries](#registry-entries)
  - [Service Configuration](#service-configuration)
- [Create a Project](#create-a-project)
  - [SQL Databases](#sql-databases)
  - [SSIS](#ssis)
  - [SSAS](#ssas)
  - [SSRS](#ssrs)
  - [Power BI](#power-bi)
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

## Create a Project

a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
## MSI Installer 
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
## Heading 2
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
a  
## heading 3






