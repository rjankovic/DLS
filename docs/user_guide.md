# User Guide

This describes the features of the "DLS Manager", the main client app of DLS. It can be found on the desktop or in the client folder of the install directory as DLS.Manager.exe.

## Contents

- [Select Project](#select-project)
- [Source - Target Flow](#source---target-flow)
  - [Selection](#selection)
  - [Lineage Map](#lineage-map)
  - [Flow Detail](#flow-detail)
  - [Element View](#element-view)
- [Overview Screen](#overview-screen)
- [Search](#search)
  - [Search Results](#search-results)
- [Data Sources](#data-sources)
- [Warnings](#warnings)
- [Business Dictionary](#business-dictionary)
  - [Fields](#fields)
  - [Views](#views)
  - [Business Links](#business-links)
- [Model Update](#model-update)

## Select Project
If you haven't created a project yet, you need to first configure one - see [this section in the installation guide](https://rjankovic.github.io/DLS/install_guide#create-project).
  
After you've configured your project, it will show up in the project list after launching the client. Alternatively, you can go to Project > Open to switch between projects.
  
![image](https://user-images.githubusercontent.com/2221666/167940535-e97c3c28-e100-427f-9c02-355d3255988d.png)

## Source - Target  Flow
  
This is one of the most useful tools of DLS. You start by selecting the part of your solution from which and to which you want to explore the lineage. The app then lists all the dataflow connections that were found going from the source componnet (e.g. the stage database) to the destination (e.g. a Power BI Report). This way you can find all the lineage that leads to the report.

### Selection

For example, this selection would find all the data flows leading from SampleDSA to Report1.  
  
![image](https://user-images.githubusercontent.com/2221666/167944261-256cec46-660f-414b-9bcb-688ba0c3be0e.png)  
  
Conversely, if you want to find all the uses of the SampleDSA.CPS.address table in SSAS model, you caould use this selection to perform impact analysis.  
  
![image](https://user-images.githubusercontent.com/2221666/167944519-2fca5f4e-1767-47d5-98d4-a1784600be5a.png)

Or you can just select the whole soure DB and target DB to view all the lineage between the 2...depends on what you need.

You've noticed that the source-target panel has multiple tabs - Root Selection, Type Selection, Lineage Map and others. The idea is that you make you way from left to right: selecting the source and target root sets the boundaries for the lineage search. That enables the Type Selection tab.
  
Now in the Type Selection tab you pick the type of objects between which the lineage is to be searched. For example, if you selected a SQL database as the source, you could be interested in source tables, views or columns (in both tables and views). So to set the detail level you want to view.

![image](https://user-images.githubusercontent.com/2221666/167945552-69c6d942-a215-41c8-927b-e35b10d488dd.png)

### Lineage Map
After you have selected the source roor and target and the obejct types to tract, the Lineage Map will be enabled:
  
![image](https://user-images.githubusercontent.com/2221666/167949394-dc4e1160-16fc-4f19-bdf1-8f015b4cef1c.png)


