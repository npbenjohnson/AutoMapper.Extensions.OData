﻿using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.Edm;
using System;
using System.Linq;

namespace AutoMapper.AspNet.OData
{
    internal static class ODataQueryContextExtentions
    {
        public static OrderBySetting FindSortableProperties(this ODataQueryContext context, Type type)
        {
            context = context ?? throw new ArgumentNullException(nameof(context));

            var entity = context.Model.FindDeclaredType(type.FullName) as IEdmEntityType;
            return entity is not null 
                ? FindProperties(entity) 
                : throw new InvalidOperationException($"The type '{type.FullName}' has not been declared in the entity data model.");


            static OrderBySetting FindProperties(IEdmEntityType entity)
            {
                var propertyNames = entity.Key().Any() switch
                {
                    true => entity.Key().Select(k => k.Name),
                    false => entity.StructuralProperties()
                        .Where(p => p.Type.IsPrimitive() && !p.Type.IsStream())
                        .Select(p => p.Name)
                        .OrderBy(n => n)
                        .Take(1)
                };
                var orderBySettings = new OrderBySetting();
                propertyNames.Aggregate(orderBySettings, (settings, name) =>
                {
                    if (settings.Name is null)
                    {
                        settings.Name = name;
                        return settings;
                    }
                    settings.ThenBy = new() { Name = name };
                    return settings.ThenBy;
                });
                return orderBySettings.Name is null ? null : orderBySettings;
            }

        }
    }
}
