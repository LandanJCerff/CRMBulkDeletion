# CRMBulkDeletion
CRM console app which bulk deletes cases &amp; activites


A list is generated on a run through of all cases, a specificed amount of records are retained and inserted into the 'safe' list. 
All records in the 'safe' list have a boolean field updated to be true. 
Once all records have had a run through, the app will continue to bulk delete every record where the CRM boolean field is false. 
When done, the app will then bulk delete all activites where regardingobjectid is null (no relating case). 
