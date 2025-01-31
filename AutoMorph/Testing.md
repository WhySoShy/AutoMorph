Everything that needs to be tested is written in here, and checked off when a test is written for it.

### Mapper Generation Tests
- [ ] `Mapper<TTarget>` is generating the standard mapper according to the Type argument.
- [ ] `Include` is generating the correct mapper according to `MapperType`, with the properties in the target class.
- [ ] `Include<T>` is generating the correct generic mapper according to `MapperType`, with the properties in the target Type, with type constraints on the method.
- [ ] `Exclude` is excluding the standard mapper when used on `Class | Struct | Record`.
- [ ] `Exclude` is excluding the property if attached to it.
- [ ] `MarkAsStatic` is force generating the mappers when attached.
- [ ] `PropertyAttribute(string)` is mapping the property to the input. 
