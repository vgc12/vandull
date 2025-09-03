using UnityEngine;

namespace Attributes
{
    public class RequiredAttribute : PropertyAttribute
    {
        public string ErrorMessage { get; }
        public RequiredAttribute()
        {
          
            ErrorMessage =  "This field must be assigned in the inspector!";
        }
        
    }
}