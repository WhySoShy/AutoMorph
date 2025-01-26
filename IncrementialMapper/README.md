### Analyzer Todo:
- [ ] Raise warning if there is no parameterless constructor
- [ ] Raise warning if multiple mappers has been marked on the same source class, if no key has been added.
> This can be done with generic attributes.
- [ ] Raise warning on attributes, if a class is not marked as partial, but they are trying to include a partial method.
- [ ] Raise error if nested object that should be mapped is not marked to be generated.

### Generator Todo:
- [X] Switch ModifierKind from Array to List, ReadonlyList or HashSet.
- [ ] Add support for Dictionaries
- [ ] Add support for parameter filled constructors
- [ ] Allow inputted keys, that can be used as reference points on classes when generating.
  - This could allow for more flexibility.

### Generator Ideas:
- [ ] Allow users to pass interfaces or classes, that will be used on generic mappers.
- [ ] Allow nested objects, to be included inside the mappers.
- [ ] Allow the user to set custom names for the mappers through the `SGMapper` attribute

### Add support for Collections:
- [ ] Key-Value based collections, like SortedList, Dictionary etc.

> Users should be able to mark each attribute with a key, where that key can be used to define what mapper it should be used on. </br >
> This can be useful if a user wants to use a single `source` and map to multiple classes.