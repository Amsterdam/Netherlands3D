# Code Conventions

## General

On this page, you'll find the code conventions setup for the Netherlands3D project.

They're primarily based on Microsoft's conventions, but a few specific decisions have been made by the development team.

Consistency is the main goal, but there are a few exceptions and those are also mentioned on this page.

### Basic Conventions

```C#

public class Conventions : MonoBehaviour, IExampleInterface
{
    // Comments are placed above the line/function they're explaining.
    // Public variables in PascalCase.
    public int ConventionValue;
    // Variables that are closely coupled in functionality (such as dimensions) can be declared on the same line (but not assigned).
    // This is acceptable.
    public int Width, Height, Depth;
    // This is unacceptable.
    public int X = 10, Y = 20, Z = 30;

    // Use an explicit property only if one of following situations apply:
    // 1. The value needs to be serialized in the inspector (not a functionality of properties).
    // 2. The Get and/or Set needs to execute more than their default functionality.

    // Situation 1.
    public int IntegerProperty { get { return privateValue; } private set { privateValue = value; } }

    // Situation 2.
    // If the line is getting too long or unreadable, split it up into multiple lines.
    public int OnScreenProperty 
    { 
        get { return myPrivateValue; } 
        private set { privateValue = value; UpdateInterface(privateValue); } 
    }

    // Default Situation.
    public int AutoProperty { get; private set; }

    // Serialized protected variables (with an underscore, in camelCase).
    [SerializeField] protected int _protectedValue;
    // Protected variables (also with an underscore and in camelCase).
    protected int _myProtectedValue;

    // Declare private variables explicitly as private.
    // Serialized private variables (in camelCase).
    [SerializeField] private int privateValue;
    // Private variables (also in camelCase).
    private int myPrivateValue;
    private OtherClass otherClass;

    // If a comment summarizes a design decision or the workings of multiple lines, place it underneath those lines
    // with one empty line above and two empty lines beneath if there are additional lines after the comment.
    // This is a special exception to be used only when it's really necessary.

    #region MyRegion

    // Minimize the use of regions, as they may hide the fact that the script might be getting too big.

    #endregion

    // The order for functions is as follows: 

    // Unity functions first (even though they're private), then public, protected and private functions. 
    // Private functions must be explicitly declared as such (with exception of the standard Unity functions).
    // Make sure to have Awake, Start, Update in that order. 

    // Other Unity functions go underneath Update (if used) and above self-defined functions.
    // Unused Unity functions should be removed (as they're still called and it clutters).
    // Do all setup of a class' own variables in Awake and linking to other classes in Start. 

    // This ensures setup/creation is complete before assignment.
    // If the other class is created by the first class (for example, with the new keyword), create it in Awake as well.

    void Awake()
    {
        otherClass = new OtherClass();
    }

    void Start()
    {
        Debug.Log(AssignedClass.Instance.Number);
    }

    // Removed Update as it wasn't needed. (This doesn't need to be commented.)

    // Functions use PascalCase.
    public void MyPublicFunction()
    {
        // Use if-shorthands only for early returns.
        if (true)
            return;

        if (true) return;

        // Both ways of writing the return are valid.



    }

    private void MyPersonalFunction()
    {
        // Place curly brackets on the next line, as Visual Studio draws lines to visualize the connection.
        if(myPrivateValue >= 1)
        {
            myPrivateValue -= 1;
            ConventionValue = myPrivateValue;
        }
    }

    // Function parameters use camelCase.
    // Make sure the parameter's name is descriptive enough. If it's generic, make sure that's clear as well.
    private void UpdateInterface(int newValue)
    {
        // There's no interface here, but instead of this function or within this function, we probably want to fire an event. 
    }

}


```

```C#
// Interfaces follow the standard conventions of: capital I + [PascalCase name of the interface].
public interface IExampleInterface
{

}
```

### Abstraction

```C#
// When using generics, it may help to constrain them using (where), if possible.
public abstract class Abstraction<T> : MonoBehaviour where T : MonoBehaviour
{
    protected T _generic;

    // Use virtual functions if one or more of following situations apply:
    // 1. There's functionality that has to be executed on every inheriting class.
    // 2. The function is always ran and evaluated, but inheriting classes aren't obligated to actually use it.
    //    In this case, create a virtual function with an empty body.
    protected virtual void VirtualFunction()
    {

    }

    // Use abstract functions when functionality can't be predefined, but must be implemented by inheriting classes.
    public abstract void AbstractFunction();

}

public class ConcreteClass : Abstraction<Conventions>
{
    protected override void VirtualFunction()
    {
        // When overriding the virtual function, remember to call the function on the base, if it exists.
        base.VirtualFunction();
    }
    public override void AbstractFunction()
    {
        //Do something specific to this class.
    }
}
```
