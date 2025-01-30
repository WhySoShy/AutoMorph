using System;
using System.Collections.Generic;
using AutoMorph.Internal.Constants;
using AutoMorph.Internal.Syntax.Tokens;

// ReSharper disable InconsistentNaming

namespace AutoMorph.Internal.Generator.Validators;

internal static class ValidCollections
{
    #region Collection Constants
    
    internal const string IENUMERABLE_GENERIC_COLLECTION = "System.Collections.Generic.IEnumerable<T>";
    internal const string ARRAY_CUSTOM = "Custom_Array_Handler"; 
    
    private const string IQUERYABLE_GENERIC_COLLECTION = "System.Linq.IQueryable<T>";
    private const string QUEUE_GENERIC_COLLECTION = "System.Collections.Generic.Queue<T>"; 
    private const string STACK_GENERIC_COLLECTION = "System.Collections.Generic.Stack<T>"; 
    private const string LIST_GENERIC_COLLECTION = "System.Collections.Generic.List<T>"; 
    private const string HASHSET_GENERIC_COLLECTION = "System.Collections.Generic.HashSet<T>"; 
    private const string LINKEDLIST_GENERIC_COLLECTION = "System.Collections.Generic.LinkedList<T>"; 
    
    private const string QUEUE_CONCURRENT_COLLECTION = "System.Collections.Concurrent.ConcurrentQueue<T>";
    private const string STACK_CONCURRENT_COLLECTION = "System.Collections.Concurrent.ConcurrentStack<T>";
    private const string BLOCKING_CONCURRENT_COLLECTION = "System.Collections.Concurrent.BlockingCollection<T>";
    private const string BAG_CONCURRENT_COLLECTION = "System.Collections.Concurrent.ConcurrentBag<T>";
    
    private const string LIST_IMMUTABLE_COLLECTION = "System.Collections.Immutable.ImmutableList<T>";
    private const string QUEUE_IMMUTABLE_COLLECTION = "System.Collections.Immutable.ImmutableQueue<T>";
    private const string STACK_IMMUTABLE_COLLECTION = "System.Collections.Immutable.ImmutableStack<T>";
    private const string SORTEDSET_IMMUTABLE_COLLECTION = "System.Collections.Immutable.ImmutableSortedSet<T>";
    
    
    #endregion Collection Constants
    
    #region Collection Casting
    
    internal static Dictionary<string, Func<ReferencePropertyToken, string, string>> SupportedCollections { get; } = new()
    {
        // Generic Collections
        {QUEUE_GENERIC_COLLECTION, (token, sourceReference) => GetGenericCast(QUEUE_GENERIC_COLLECTION, token, sourceReference)},
        {STACK_GENERIC_COLLECTION, (token, sourceReference) => GetGenericCast(STACK_GENERIC_COLLECTION, token, sourceReference)},
        {LIST_GENERIC_COLLECTION, (token, sourceReference) => $"{sourceReference}.{token.SourcePropertyName}.{token.NestedObject!.MethodToken.Name}().ToList()"},
        {HASHSET_GENERIC_COLLECTION, (token, sourceReference) => $"{sourceReference}.{token.SourcePropertyName}.{token.NestedObject!.MethodToken.Name}().ToHashSet()"},
        {LINKEDLIST_GENERIC_COLLECTION, (token, sourceReference) => GetGenericCast(LIST_GENERIC_COLLECTION, token, sourceReference)},
        
        // Concurrent Collections
        // Basically just Thread-Safe collections -> https://learn.microsoft.com/en-us/dotnet/standard/collections/thread-safe/
        {QUEUE_CONCURRENT_COLLECTION, (token, sourceReference) => GetGenericCast(QUEUE_CONCURRENT_COLLECTION, token, sourceReference)},
        {STACK_CONCURRENT_COLLECTION, (token, sourceReference) => GetGenericCast(STACK_CONCURRENT_COLLECTION, token, sourceReference)},
        {BLOCKING_CONCURRENT_COLLECTION, (token, sourceReference) => $"new {AssemblyConstants.GLOBAL_KEYWORD}{BLOCKING_CONCURRENT_COLLECTION}<{AssemblyConstants.GLOBAL_KEYWORD}{token.NestedObject!.Type.Key}>({GetGenericCast(QUEUE_CONCURRENT_COLLECTION, token, sourceReference)})"},
        {BAG_CONCURRENT_COLLECTION, (token, sourceReference) => GetGenericCast(BAG_CONCURRENT_COLLECTION, token, sourceReference)},
        
        // Immutable Collections
        {LIST_IMMUTABLE_COLLECTION, (token, sourceReference) => GetImmutableCast(LIST_IMMUTABLE_COLLECTION, token, sourceReference)},
        {QUEUE_IMMUTABLE_COLLECTION, (token, sourceReference) => GetImmutableCast(QUEUE_IMMUTABLE_COLLECTION, token, sourceReference)},
        {STACK_IMMUTABLE_COLLECTION, (token, sourceReference) => GetImmutableCast(STACK_IMMUTABLE_COLLECTION, token, sourceReference)},
        {SORTEDSET_IMMUTABLE_COLLECTION, (token, sourceReference) => GetImmutableCast(SORTEDSET_IMMUTABLE_COLLECTION, token, sourceReference)},
        
        // Custom Collection Casts, this is just something that is being used by the Generator itself.
        {ARRAY_CUSTOM, (token, sourceReference) => $"{sourceReference}.{token.SourcePropertyName}.{token.NestedObject!.MethodToken.Name}().ToArray()"},
    };
    
    /// <summary>
    /// Can be used by the <see cref="SupportedCollections" /> to reuse the way that the generic collections is being instantiated. 
    /// </summary>
    private static string GetGenericCast(string collectionTypeName, ReferencePropertyToken token, string sourceReference)
        => $"new {AssemblyConstants.GLOBAL_KEYWORD}{collectionTypeName.Replace("<T>", $"<{AssemblyConstants.GLOBAL_KEYWORD}{token.NestedObject!.Type.Key}>")}({sourceReference}.{token.SourcePropertyName}.{token.NestedObject!.MethodToken.Name})";
    
    private static string GetImmutableCast(string collectionTypeName, ReferencePropertyToken token, string sourceReference)
        => $"{AssemblyConstants.GLOBAL_KEYWORD}{collectionTypeName}.CreateRange({sourceReference}.{token.SourcePropertyName}.{token.NestedObject!.MethodToken!.Name}())";
    
    #endregion
}