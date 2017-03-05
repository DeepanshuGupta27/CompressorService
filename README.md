# Compressor Service 

CompressorService Solution consists of below Projects:

* CompressorService : RESTFUL Web API for compressing images provided in below formats:
 * Image URL
 * List of Image URL's
 * CSV or XLSX file which contains Image URL's
 
* ImageCompressor : Library for compressing images.

* CompressorService.Tests : Unit test cases for Web API and Library.


## Getting Started

Follow below instructions for setting up and running this project on your local machine for 
development and testing this project.

### Prerequisites

* .NET Framework 4.5.2 or above.
* Visual Studio

### Project Setup

* Clone Repository
> https://github.com/DeepanshuGupta27/CompressorService.git

* Open CompressorService.sln in Visual Studio

###Deployment

* Build entire solution in Visual Studio

* Once build succeeded, make CompressorService project as a startup project.

* Start an instance of this project.

Your service will be deployed on http:localhost:portNumber.

Append ***/swagger*** to the deployed address for exploring Compressor Service.


