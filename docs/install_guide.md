# DLS Installation

This guide describes how to install Data Lineage Services and configure a data lineage project.

## Contents

- [MSI Installer](#msi-installer)
  - [Folder Structure](#folder-structure)
- [Configuration App](#configuration-app)
  - [Connecting to DB](#connecting-to-db)
  - [Registry Entries](#registry-entries)
  - [Service Configuration](#service-configuration)
- [Create a Project](#creating-a-project)
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
 - **extractor** is a console app for extracting the metadata of databases, reports, etc.. It saves these metadata in text files, which are then processed by the DLS service
 - **service** is the backend of the app - it parses the metadata and provides lineage info to client apps. It can run as a Windows service or as a background process within the client or as an independent Windows service. (see [Service Configuration](#service-configuration))


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






