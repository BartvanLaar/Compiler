
namespace Lexing
{
    public enum TypeIndicator
    {
        None,
        Inferred,
        //UserDefined, // we don't know if a type is user defined as we only know that it's an identifier, not whether it's a type.
        Float, // should return types be their own enum type? and replaced as a token called ReturnType?
        Double,
        Boolean,
        Integer,
        Character,
        String,
        DateTime, // !notimplemented
        Void,
        //Hexadecimal, // Is actually just a different way to type an integer? But would be kinda cool to be its own type perhaps
    }
}
