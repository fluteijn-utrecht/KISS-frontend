## Meerdere zaaksystemen
- Vanwege grote hoeveelheid endpoints handelen we zovool mogelijk calls centraal af
- Voor lijsten willen we de respons van alle zaaksystemen samenvoegen en onderling sorteren
  - Als een lijst uit een zaaksysteem lukt, maar een ander zaaksysteem stuk is, loggen we dit maar geven we de lijst alsnog terug aan de frontend
- Waar mogelijk willen we alleen zoeken in het betreffende zaaksysteem. Dit betkent dat er bij een initiele call per zaak context meegegeven zal moeten worden over het zaaksysteem in de vorm van een header, zodat de frontend deze bij latere verzoeken kan doorsturen.
  - dit doen we omdat we verwachten dat de foutafhandeling hier eenvoudiger van wordt.