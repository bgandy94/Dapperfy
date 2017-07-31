# Dapperfy
Script generation helper for Dapper .NET core and .NET 4.6

## Core Functionality
1. CRUD Script generation based on C# models.
2. Custom attributes to notify the library of a Primary Key

## Usage
 1. Add Dapperfy to your current project (obviously one of the requirements of using Dapperfy is to have Dapper installed.)
 2. Create a model to be used by Dapper that matches your table in the database. Ex below:
 ```
  public class Customer
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        public string EmailAddress { get; set; }
    }
 ```
 3. To tell Dapperfy which field is the Primary Key, add the ```[PrimaryKey]``` attribute to the field. Ex below:
 ```
        [PrimaryKey]
        public int Id { get; set; }
 ```
 4. You should be setup! Dapperfy can be called directly from your code to generate the queries that will be used by Dapper. Ex below:
 ```
        string script = Dapperfy.Add(customer)
 ```
