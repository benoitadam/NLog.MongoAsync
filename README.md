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
	
### Using custom formatting	

TODO

### Target Settings:

* Host (Defaults to 'localhost')
* Port (Defaults to 27017)
* Database(Defaults to 'NLog')
* Username
* Password
* ConnectionString (a complete Mongo Url)
* ConnectionName (the name configured in the configuration/connectionString node)
* CollectionName - the name of the Mongo collection. If you don't specify this, it uses the logger name.
