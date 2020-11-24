# StadsArkivet
Assignment for job interview

Created as a basic .net core console application with no other dependencies.

**To run the program:** 
clone the repository and use visual studio run the program.
It will open a console window with build events and status in the end.

**The status will show success status for each database table**
  - true  = All rows' elements matched the table definition.
  - false = One or more elements did not match the table definition
  
  
**How code functions**
1 - The code will read the .xml file containing the indicies for database tables.
2 - It finds the tables section in the file
3 - Goes through each table and find column definition
4 - saves it all in a dictionary<tableName, Dictionary<columnName, dataType>>
5 - Read each database table from .xml files
6 - Go through each row and find matching table, columnName in the dictionary
7 - Compare element's value's datatype with and data type definitions from dictionary
8 - Set success to false if it find something that does not match
9 - Show success status for each table in console output.
