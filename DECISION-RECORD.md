## Meerdere zaaksystemen
- Vanwege grote hoeveelheid endpoints in de ZGW standaard handelen we het bevragen van meerdere zaaksystemen zoveel mogelijk centraal en generiek af
- Voor lijsten van entiteiten voegen we de respons van alle zaaksystemen samen en sorteren het resultaat
  - Als er meerdere zaaksystemen bevraagd worden voor lijsten van entiteiten, en er een zaaksysteem niet benaderbaar is of een onverwacht antwoord geeft:
    - loggen we dit
    - negeren we dit in de respons naar de frontend
- Waar mogelijk willen we alleen zoeken in het betreffende zaaksysteem waar een resultaat verwacht kan worden.
  - Dit doen we door per zaak mee te geven uit welk zaaksysteem deze komt
  - Deze informatie sturen we in vervolgcalls door in een header: `ZaaksysteemId`
  - Dit doen we om irrelevante foutmeldingen te voorkomen