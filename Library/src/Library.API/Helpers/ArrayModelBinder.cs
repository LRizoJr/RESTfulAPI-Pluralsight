using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Helpers
{
    public class ArrayModelBinder : IModelBinder
    {
        // Binds our custom id collection route template to the api controller model
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            // Check for compatibility with our expected template format (IEnumerable)
            if(!bindingContext.ModelMetadata.IsEnumerableType)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            // Get the input template value -- should be a list of Guids
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).ToString();

            // return null if the value is empty or null
            if(string.IsNullOrEmpty(value.Trim()))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            // Valid value - get the type of the Enumerable and a converter for it
            var elementType = bindingContext.ModelType.GenericTypeArguments[0];
            var converter = TypeDescriptor.GetConverter(elementType);


            // Split the string into a string array, then convert the string array to an array of Guids using the Converter 
            var values = value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => converter.ConvertFromString(x.Trim())).ToArray();
            var typedValues = Array.CreateInstance(elementType, values.Length);
            values.CopyTo(typedValues, 0);

            bindingContext.Model = typedValues;
            bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
            return Task.CompletedTask;
        }
    }
}
