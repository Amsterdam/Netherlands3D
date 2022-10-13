# Project Structure

### General

We organise the project by defining a few rules that declare how elements should be saved and where they should be saved.

Unity already comes with it's Assets folder and, within it, a Scene folder. This root Assets folder should stay uncluttered, meaning it should not contain many loose assets within it. Having a single Render Pipeline in there is acceptable, of course.

### Scripts

Scripts within the same namespace should be bundled in a shared folder and scripts should share a namespace if they're clearly made to support or work with each other. The use of namespaces is encouraged regardless, so that autocomplete doesn't expose many classes that are irrelevant in other scripts.

Contrary to what's regularly seen within game development, the use of "Managers" is discouraged as it creates ambiguity with regard to the actual function of the script. So make sure that whatever is created has a clear function and a name that conveys clearly what that function is.

### Packages

As the core idea for Netherlands3D is to have a base layer upon which functionality can be built, those layers should mostly work independently from other layers. This means that elements a layer is dependent upon should be a part of that layer's package in most cases.

If a situation materializes where multiple layers require the same script(s), it's better to move it/them to a package and namespace of their own. After that, make sure the layers require this new package to be imported if either of them is imported. This makes sure that the layers can be imported independently, but it also prevents duplicate scripts if both are imported at the same time.





Additional information and example images to be added in the future.
