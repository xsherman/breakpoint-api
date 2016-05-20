﻿using Lisa.Common.WebApi;
using System.Linq;

namespace Lisa.Breakpoint.Api
{
    public class MemberShipsValidator : Validator
    {
        private void ValidateRole(string fieldName, object value)
        {
            string[] status = new string[] { "developer", "tester", "manager"};

            if (value != null)
            {
                if (!status.Contains(value))
                {
                    var error = new Error
                    {
                        Code = ErrorCode.EmptyValue,
                        Message = $"In field '{fieldName}' only the values: developer, tester and manager are allowed.",

                        Values = new
                        {
                            Field = fieldName,
                            Value = value,
                            ExpectedValues = status
                        }
                    };
                    Result.Errors.Add(error);
                }
            }
        }

        protected override void ValidateModel()
        {
            Ignore("id");
            Required("userName", NotEmpty, TypeOf(DataTypes.String));
            Required("project", NotEmpty, TypeOf(DataTypes.String));
            Required("role", NotEmpty, ValidateRole, TypeOf(DataTypes.String));
        }

        protected override void ValidatePatch()
        {

        }
    }
}