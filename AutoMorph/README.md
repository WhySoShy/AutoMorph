### Current features
- > Mappers can be injected into partial classes.
- > Mappers can be generated as extension methods.
- > Mappers can be generated as generic methods, by passing an interface to the `IncludeAttribute`
- > Properties can map into other properties with different names
- > Current supported mapper types -> IEnumerable, IQueryable, Object
- > Ignore property from mapping
  
### Analyzer Todo:
- [ ] Raise warning if there is no parameterless constructor
- [ ] Raise warning if multiple mappers has been marked on the same source class, if no key has been added.
> This can be done with generic attributes.
- [ ] Raise warning on attributes, if a class is not marked as partial, but they are trying to include a partial method.
- [ ] Raise warning if the source or target class does not contain any properties.
- [ ] Raise Info if the source class is of abstract type, and `IsGeneric` is set to true.
- [ ] Raise error if the source property does not contain a visible getter.
- [ ] Raise error if the target property does not contain a visible setter.
- [ ] Raise error if nested object that should be mapped is not marked to be generated.
- [ ] Raise error if the target class is an abstract class.

### Generator Todo:
- [X] Switch ModifierKind from Array to List, ReadonlyList or HashSet.
- [x] Ensure the source property is readable, and target property is settable.
- [ ] Use caching for retrieving GetTypeDeclaration.
- > This can be used by using [MSBuildWorkspace](https://www.nuget.org/packages/Microsoft.CodeAnalysis.Workspaces.MSBuild)

### Support Todo:
- [ ] Add support for Internal, protected, internal protected classes.
- [ ] Add support for Dictionaries
- [ ] Add support for parameter filled constructors
- [ ] Nested objects should be able to be created as type parameters

### Generator Ideas:
- [x] Allow users to pass interfaces or classes, that will be used on generic mappers.
- [x] Allow nested objects, to be included inside the mappers.
 > The mapper generates a mapper for the nested object, this allows for deep nesting within an object.
- [x] Allow mapping from between property types
- [ ] Config files
- > This would allow users to set a default mapping configuration, that will be applied to all the mappers within the project or assembly.

### Attribute Ideas:
- [ ] Set nesting level
- [ ] Add support for reverse mapping

### Add support for Collections:
- [ ] Key-Value based collections, like SortedList, Dictionary etc.``

> Users should be able to mark each attribute with a key, where that key can be used to define what mapper it should be used on. </br >
> This can be useful if a user wants to use a single `source` and map to multiple classes.``