# Unit tests

Unit tests for the front-end are written if deemed appropriate by the developer making a code change and/or the developer reviewing a code change. Code coverage is explicitely NOT deemed a useful metric for this project.

## Possible criteria for a unit of code that merits unit tests
- A unit of code that, when not properly implemented, could result in critical failures that are difficult to detect.
- A unit of code that's difficult to reason about without runnable examples.
- A unit of code that's specifically meant for widespread re-use.

## Criteria to keep in mind when writing unit tests
[based on this article](https://www.ponicode.com/shift-left/a-beginner-guide-to-understanding-unit-testing)
- **Independent**: unit tests should test an isolated unit of code. Mocks should only be used if there is no feasable alternative.
- **Repeatable**: unit tests should be deterministic.
- **Readable**: when a test fails, it needs to be easy to identify the point of failure. 
- **Exhaustive**: unit tests should target an exhaustive range of scenarios.
- **Maintainable**: unit tests should NOT be tightly coupled to the implementation of the unit of code it is meant to test.

## Current candidates for Front-end unit tests
In order of priority:
- [ ] [`/src/services/service-data.ts`](/src/services/service-data.ts)
  - [ ] `ServiceResult.fromPromise`: test if the correct result type is returned for a pending, a resolved and an rejected Promise.
  - [ ] `ServiceResult.fromSubmitter`: test if the starting state is init, test for the three stages of a Promise on the submit function, test if the state goes back to init after the reset function is called.
  - [ ] `ServiceResult.fromFetcher`: Test for the three stages of a Promise on the fetcher, test if the unique id is preferred over the url, test for the loading boolean: does it initialize as false if there's a value in the cache? Does it switch to true if the uniqueId changes?
- [ ] [`/src/services/service-data-enricher.ts`](/src/services/service-data-enricher.ts): test with two sample enrichers, for the following scenarios:
  - [ ] start with an object of type `Left`, NOT having the necessary key to retrieve data of type `Right`
  - [ ] start with an object of type `Right`, NOT having the necessary key to retrieve data of type `Left`
  - [ ] start with an object of type `Left`, having the necessary key to retrieve data of type `Right`. The enricher returns a resolved promise
  - [ ] start with an object of type `Left`, having the necessary key to retrieve data of type `Right`. The enricher returns a forever pending promise
  - [ ] start with an object of type `Left`, having the necessary key to retrieve data of type `Right`. The enricher returns a rejected promise
  - [ ] start with an object of type `Right`, having the necessary key to retrieve data of type `Left`. The enricher returns a resolved promise
  - [ ] start with an object of type `Right`, having the necessary key to retrieve data of type `Left`. The enricher returns a forever pending promise
  - [ ] start with an object of type `Right`, having the necessary key to retrieve data of type `Left`. The enricher returns a rejected promise
- [ ] [`/src/services/gateway.ts`](/src/services/gateway.ts)
  - [ ] `parsePagination`: test with an invalid pagination object, a pagination object with an empty result array, a synchronous mapper and an asynchronous mapper
- [ ] [`/src/services/fetch-logged-in.ts`](/src/services/fetch-logged-in.ts)
  - [ ] test if a non-401 response is just returned
  - [ ] test if a 401 response triggers a retry after calling `handleLogin`
- [ ] [`/src/stores/create-store.ts`](/src/stores/create-store.ts)
- [ ] [`/src/helpers/validation.ts`](/src/helpers/validation.ts)
  - [ ] `parseDutchDate`: check for valid cases: `dd-MM-yyyy`, `d-MM-yyyy`, `dd-M-yyyy`, `dd/MM/yyyy`, `d/MM/yyyy`, `dd/M/yyyy`, `d/M/yyyy`, `ddMMyyyy`, as well as an invalid case
  - [ ] `parseBsn`: test for a list of known valid/invalid bsns
- [ ] [`/src/nl-design-system/components/Pagination.vue`](/src/nl-design-system/components/Pagination.vue): test for each case handled by the component, such as an empty result, a single page, less than 5 pages, more than 5 pages, with the current page beeing the first, last, or somewhere in between.
  

## Less priority:
- [ ] all code in [`/src/helpers`](/src/helpers)
- [ ] all code in [`/src/features/shared`](/src/features/shared)


## Current candidates for Back-end unit tests
Bestaat grotendeels uit (proxy)configuratie en daardoor beperkt geschikt voor unittests 

In order of priority:
### Verwerkingslog middleware
Belangrijke functionaliteit die bij elk contactmoment uitgevoerd moet worden. daardoor waardevol om te testen
### ZGW api's
Grotendeels proxyconfiguratie die niet geschikt is voor unittests. een paar uitzonderingen:
- het verrijken van een op te slaan contactmoment met de gegevens van de gebruiker
- het creeeren van een autorisatie token
### Beheerfucntionaliteit
- Gespreksresultaten CRUD acties
- Links CRUD acties
- Skills CRUD acties 
- Nieuws en werkinstruncties CRUD acties
- Belangrijke berichten aantal ophalen
- Berichten als gelezenmarkeren
- Berichten zoeken 
### Feedbackversturen
- Het opstellen van de mail is testbaar. het versturen niet
