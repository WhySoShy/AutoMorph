### Current features
- > Mappers can be injected into partial classes.
- > Mappers can be generated as extension methods.
- > Properties can map into other properties with different names
- > Current supported mapper types -> IEnumerable, IQueryable, Object
- > Ignore property from mapping

### Analyzer Todo:
- [ ] Raise warning if there is no parameterless constructor
- [ ] Raise warning if multiple mappers has been marked on the same source class, if no key has been added.
> This can be done with generic attributes.
- [ ] Raise warning on attributes, if a class is not marked as partial, but they are trying to include a partial method.
- [ ] Raise error if nested object that should be mapped is not marked to be generated.

### Generator Todo:
- [X] Switch ModifierKind from Array to List, ReadonlyList or HashSet.
- [ ] Allow inputted keys, that can be used as reference points on classes when generating.
  - This could allow for more flexibility.
- [ ] Ensure abstract classes cannot be mapped on.

### Support Todo:
- [ ] Add support for Internal, protected, internal protected classes.
- [ ] Add support for Dictionaries
- [ ] Add support for parameter filled constructors

### Generator Ideas:
- [ ] Allow users to pass interfaces or classes, that will be used on generic mappers.
- [x] Allow nested objects, to be included inside the mappers.
 > The mapper generates a mapper for the nested object, this allows for deep nesting within an object.
- [ ] Allow the user to set custom names for the mappers through the `SGMapper` attribute
- [ ] Allow mapping from between property types,
- [ ] Allow `MarkAsStatic` attribute to be appended on partial methods, to allow static methods


### Attribute Ideas:
- [ ] Order nested collections
- [ ] Set nesting level
- [ ] Add support for reverse mapping

### Add support for Collections:
- [ ] Key-Value based collections, like SortedList, Dictionary etc.

> Users should be able to mark each attribute with a key, where that key can be used to define what mapper it should be used on. </br >
> This can be useful if a user wants to use a single `source` and map to multiple classes.