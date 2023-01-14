# Overview 

D365FO Data CSDL Shortener is a command line tool that makes it easy to make a CSDL file shorter.

## Capabilities

D365FO Data CSDL Shortener can slice CSDL document by applying filtration by a public entity name(s) and/or data entity actions.
This is useful for scenarios when it is necessary to convert a CSDL $metadata file from Dynamics 365 Finance and SCM (D365F&O/SCM) to other formats, such as OpenAPI or C# proxy classes, and do not break converter because of huge $metadata file size.

The tool keeping the specified data entity(es) and/or data entity actions from the CSDL, and also cleaning orphaned file entries that is unused in converters and scaffolders. In other words it cleaning the orphaned tags from the CSDL file and significantly reducing the size of CSDL.

 
## Installation 

Download from GitHub, compile and use: https://github.com/MazZzDaI/D365ODataCsdlShortener
 
 
## How to use D365ODataCsdlShortener
Once you've compiled/installed the package locally, you can invoke the D365FO Data CSDL Shortener by running: D365ODataCsdlShortener [command]. 
You can access the list of command options we have by running D365ODataCsdlShortener -h 
The tool avails the following commands: 
	• --csdlSourceFilePath(-cs) - CSDL file path in the local filesystem or a valid URL hosted on an HTTPS server 
	• --csdlEntitySetFilter(-csfe) - a filter parameter that user can use to select a subset of data entities from a large CSDL file. By providing a comma separated list of EntitySet names that appear in the EntityContainer
	• --csdlActionFilter(-csfa) - a filter parameter that user can use to select a subset of data entity actions from a large CSDL file. By providing a comma separated list of Action names
	• --csdlTransformedFilePath(-o) - Output directory path for the transformed document 
	• --loglevel(-ll) - The log level to use when logging messages to the main output 
 
 **Examples:**  

	1. Get local $metadata.xml file and keep only VendorPaymentJournalLines and CustomersV2 data entity, also to keep getDirects and RunDocumentAction actions 
	D365ODataCsdlShortener -cs csdlMetadataShort.xml -csfe VendorPaymentJournalLines,CustomersV2 -csfa getDirects,RunDocumentAction -o metadataFiltered.xml
    
    2. Get local $metadata.xml file and keep only CustomersV2 data entity, also keep all actions 
	D365ODataCsdlShortener -cs https://d365devaos.axcloud.dynamics.com/data/`$metadata -csfe CustomersV2 -o metadataFiltered.xml

