using UnityEngine;

public class PlayInteraction : MonoBehaviour
{
    //! https://www.youtube.com/watch?v=_yf5vzZ2sYE&t=229s
    //! https://www.youtube.com/watch?v=cxJnvEpwQHc&t=674s

    private Transform _currentSelection;

    private ISelector _selector;
    private IRayProvider _rayProvider;
    private ISelectionResponse _slectionResponse;

    // Start is called before the first frame update
    void Awake()
    {
        _selector = GetComponent<ISelector>();
        _rayProvider = GetComponent<IRayProvider>();
        _slectionResponse = GetComponent<ISelectionResponse>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_currentSelection != null) _slectionResponse.OnDeselect(_currentSelection);

        _selector.Check(_rayProvider.CreateRay());
        _currentSelection = _selector.GetSelection();

        if (_currentSelection != null) _slectionResponse.OnSelect(_currentSelection);
    }
}