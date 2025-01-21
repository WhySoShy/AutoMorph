### Analyzer Todo:
- [ ] Raise warning if there is no parameterless constructor
- [ ] Raise warning if multiple mappers has been marked on the same source class, if no key has been added.
> This can be done with generic attributes. 

### Generator Ideas:
- [ ] Allow users to pass interfaces or classes, that will be used on generic mappers.

> Users should be able to mark each attribute with a key, where that key can be used to define what mapper it should be used on. </br >
> This can be useful if a user wants to use a single `source` and map to multiple classes.