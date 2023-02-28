using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;

public class EventContainerGraphWindow : GraphViewEditorWindow
{
    [MenuItem("Window/Event Container Graph")]
    public static void OpenWindow()
    {
        var window = GetWindow<EventContainerGraphWindow>();
        window.titleContent = new GUIContent("Event Container Graph");
    }

    private GraphView graphView;

    private void OnEnable()
    {
        graphView = new GraphView(this);
        graphView.name = "Event Container Graph";
        graphView.StretchToParentSize();

        // Add the toolbar to the graph view
        Toolbar toolbar = new Toolbar();
        graphView.Add(toolbar);

        // Add a button to create new nodes
        Button createNodeButton = new Button(() =>
        {
            var node = new EventContainerNode();
            node.SetPosition(new Rect(10, 10, 100, 100));
            graphView.AddElement(node);
        });
        createNodeButton.text = "Create Node";
        toolbar.Add(createNodeButton);

        // Add nodes for every ScriptableObject that extends from EventContainer
        foreach (var assetPath in AssetDatabase.FindAssets("t:ScriptableObject"))
        {
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(assetPath));
            if (asset is EventContainer eventContainer)
            {
                var node = new EventContainerNode(eventContainer);
                node.SetPosition(new Rect(eventContainer.Position, Vector2.zero));
                graphView.AddElement(node);
            }
        }

        // Add the graph view to the window
        rootVisualElement.Add(graphView);
    }

    private void OnDisable()
    {
        // Remove all elements from the graph view to avoid memory leaks
        graphView.DeleteElements(graphView.nodes.ToList());
        graphView.DeleteElements(graphView.edges.ToList());
    }
}

public class EventContainerNode : Node
{
    public EventContainer eventContainer;

    public EventContainerNode() : base("Event Container")
    {
        // Add input and output ports to the node
        var inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(EventContainer<));
        inputPort.portName = "Input";
        inputContainer.Add(inputPort);

        var outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(EventContainer));
        outputPort.portName = "Output";
        outputContainer.Add(outputPort);
    }

    public EventContainerNode(EventContainer eventContainer) : this()
    {
        // Save the EventContainer reference and update the node's title
        this.eventContainer = eventContainer;
        title = eventContainer.name;
    }
}
