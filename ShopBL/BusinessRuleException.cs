using System;

namespace ShopBL;

public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}