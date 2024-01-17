namespace Net.Arqsoft.QsMapper.Exceptions;

using global::System;

class InvalidMapTypeException : Exception
{
    public InvalidMapTypeException(string message) : base(message) { }
}