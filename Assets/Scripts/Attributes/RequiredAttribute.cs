using UnityEngine;

namespace Attributes
{
    public class RequiredAttribute : PropertyAttribute
    {
        public RequiredAttribute()
        {
            ErrorMessage = "This field must be assigned!";
        }

        public string ErrorMessage { get; }
    }
}