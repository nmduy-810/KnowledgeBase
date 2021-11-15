using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace KnowledgeBase.Utilities.Helpers
{
    public class ApiBadRequestResponse: ApiResponse
    {
        public IEnumerable<string> Errors { get; set; }
        
        public ApiBadRequestResponse(ModelStateDictionary modelStateDictionary) : base(400)
        {
            if (modelStateDictionary.IsValid)
            {
                throw new ArgumentException("Model must be invalid", nameof(modelStateDictionary));
            }

            Errors = modelStateDictionary
                .SelectMany(x => x.Value.Errors)
                .Select(x => x.ErrorMessage).ToArray();
        }

        public ApiBadRequestResponse(IdentityResult identityResult) : base(400)
        {
            Errors = identityResult.Errors.Select(x => x.Code + "-" + x.Description).ToArray();
        }

        public ApiBadRequestResponse(string message) : base(400, message)
        {
        }
    }
}