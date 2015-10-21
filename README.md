NLog.MongoAsync Target
=============
In development

An NLog Target for MongoDB.Driver 2, asynchronous and Bulk insert.

The NLog.MongoAsync target allows you to use a MongoDB instance as the persistence mechanism for NLog.

You can define which database and server to use, but by default a collection will be created for you use.

Installation
-------------

To install, place the binaries in your application bin and add the following configuration entries to your NLog configuration.

### Using localhost connection string

	<?xml version="1.0" encoding="utf-8" ?>
	<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
			xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
		<extensions>
			<add assembly="NLog.MongoAsync"/>
		</extensions>
		<targets async="true">
			<target name="Mongo" type="MongoAsync" />
		</targets>
		<rules>
			<logger name="*" minLevel="Debug" appendTo="Mongo"/>
		</rules>
	</nlog>
	
### Using a new connection string

	<?xml version="1.0" encoding="utf-8" ?>
	<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
			xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
		<extensions>
			<add assembly="NLog.MongoAsync"/>
		</extensions>
		<targets async="true">
			<target name="Mongo" type="MongoAsync" connectionString="mongodb://mongo:db@server:12345/nlog" />
		</targets>
		<rules>
			<logger name="*" minLevel="Debug" appendTo="Mongo"/>
		</rules>
	</nlog>	

### Using integrated settings

	<?xml version="1.0" encoding="utf-8" ?>
	<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
			xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
		<extensions>
			<add assembly="NLog.MongoAsync"/>
		</extensions>
		<targets async="true">
			<target name="Mongo" type="MongoAsync" 
				database="nlog"
				host="server"
				port="12348"            
				username="mongo"
				password="password"/>
		</targets>
		<rules>
			<logger name="*" minLevel="Debug" appendTo="Mongo"/>
		</rules>
	</nlog>
	
### Target Settings:

* CollectionName : The name of the Mongo collection. (Defaults to entry assembly name or 'Other' if null)
* ConnectionString : The connection string name string. (Defaults to 'mongodb://localhost')
* DatabaseName : The name of the database. (Defaults to 'logs')
* UseCappedCollection : A value indicating whether to use a capped collection. (Defaults to 'true')
* CappedCollectionSize : The size in bytes of the capped collection. (Defaults to '8589934592' = 1Go)
* CappedCollectionMaxItems : The capped collection max items. (Defaults to 'null')
* UseFormattedMessage : A value indicating whether to use the default message formating. (Defaults to 'true')

### About this repository

This repository is inspired by :
* Logrythmik/NLog.MongoDB
* loresoft/NLog.Mongo

I wrote this repository for optimized the time of logging.
In the next step I will work on log viewer.
