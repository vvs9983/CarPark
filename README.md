# Car Park Management API
A simple car park management system built with .NET 8 and Entity Framework Core (InMemory).
The API supports allocating vehicles to parking spots, check occupancy and calculating charges on exit.

# Features
  *  Allocate vehicle to the first free spot
  *  Occupancy status
  *  Deallocate a spot on car exit
  *  Calculate charge
  *  Rates, surcharge and car types are stored in the database
  *  Includes minimal unit tests

# Setup and Run Locally
**Prerequisites**
  *  .NET 8 SDK

**Clone and Restore**

<pre>
  bash
  
  git clone https://github.com/vvs9983/CarPark.git
  cd carpark.api
  dotnet restore
</pre>

**Run the API**

<pre>
  bash

  dotnet run -project CarPark.Api
</pre>

The API will start at `localhost:5256`

**Test with Awagger**

Browse to: `http://localhost:5256/swagger`

#

**Database**
  *  InMemory EF Core provider for demo use

#

**Testing**

Run tests with:

<pre>
  bash

  dotnet test
</pre>

#

**Error Handling**
  *  400 Bad Request -> Missing/Invalid body
  *  404 Not Found -> Vehicle not parked
  *  409 Conflict -> No free spots/Duplicate allocation

#

**Assumprions**
 * One vehicle registration can occupy one spot at a time
 * Time calculations are in UTC
 * Charges are rounded to 2 decimal places.
