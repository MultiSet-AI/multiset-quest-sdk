using UnityEngine;
using TMPro;
using MultiSet;

/**
 * Handles the navigation UI state and input.
 */
public class NavigationUIController : MonoBehaviour
{
    public static NavigationUIController instance;

    [Tooltip("Label to show remaining distance")]
    public TextMeshProUGUI remainingDistance;

    [Tooltip("Label to show name of current destination")]
    public TextMeshProUGUI destinationName;

    [Tooltip("Parent GameObject of navigation progress slider")]
    public GameObject navigationProgressSlider;

    [Tooltip("Button to stop navigation")]
    public GameObject stopButton;

    [Space(10)]
    [Tooltip("SelectList where POIs are shown")]
    public SelectList poiList;

    [Tooltip("Parent GameObject of POIs selection UI")]
    public GameObject DestinationSelectUI;

    [Tooltip("Navigation Path Material")]
    private Material material;

    [Space(20)]
    [SerializeField] private OVRInput.RawButton m_openListButton = OVRInput.RawButton.B;

    [SerializeField] private OVRInput.RawButton m_stopNavigationButton = OVRInput.RawButton.X;


    [SerializeField]
    private GameObject m_poiListUI;
    [SerializeField]
    private GameObject m_cameraRig;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        ShowNavigationUIElements(false);
        DestinationSelectUI.SetActive(false);

        destinationName.text = "";

        material = Resources.Load<Material>("Materials/NavPathMaterial");
    }

    void Update()
    {
        HandleNavigationState();
        UpdateRemainingDistance();

        if (OVRInput.GetDown(m_openListButton))
        {
            ToggleDestinationSelectUI();

            m_poiListUI.SetActive(true);

            // Populate POI list UI in front of the user
            m_poiListUI.transform.position = m_cameraRig.transform.localPosition + m_cameraRig.transform.forward * 0.6f;
            m_poiListUI.transform.rotation = Quaternion.LookRotation(m_cameraRig.transform.forward, Vector3.up);
        }

        if (OVRInput.GetDown(m_stopNavigationButton))
        {
            ClickedStopButton();
        }

    }

    // handles the 
    void HandleNavigationState()
    {
        if (NavigationController.instance.IsCurrentlyNavigating())
        {
            destinationName.text = NavigationController.instance.currentDestination.poiName;
            return;
        }
        destinationName.text = "";
    }

    /**
     * Toggles visibility of destination select UI.
     */
    public void ToggleDestinationSelectUI()
    {
        DestinationSelectUI.SetActive(!DestinationSelectUI.activeSelf);

        if (!DestinationSelectUI.activeSelf)
        {
            poiList.ResetPOISearch();
            return;
        }

        poiList.RenderPOIs();
    }

    public void ResetPoiSearch()
    {
        poiList.ResetPOISearch();
    }

    public void RenderPoiCall()
    {
        poiList.RenderPOIs();
    }

    // User clicked to start navigation. Is called from ListItemUI.cs
    public void ClickedStartNavigation(POI poi)
    {
        NavigationController.instance.SetPOIForNavigation(poi);
        ToggleDestinationSelectUI();

        ShowNavigationUIElements(true);
    }

    // User clicked to stop navigation
    public void ClickedStopButton()
    {
        ShowNavigationUIElements(false);
        NavigationController.instance.StopNavigation();
    }

    // toggle visibility of navigation UI elements
    void ShowNavigationUIElements(bool isVisible)
    {
        // for navigation
        navigationProgressSlider.SetActive(isVisible);
        stopButton.SetActive(isVisible);
    }

    // Update info about remaining distance.
    void UpdateRemainingDistance()
    {
        if (!NavigationController.instance.IsCurrentlyNavigating())
        {
            remainingDistance.SetText("");
            return;
        }

        int distance = PathEstimationUtils.instance.getRemainingDistanceMeters();
        string distanceText = distance + "";

        if (distance > 1)
        {
            if (material != null)
                material.SetFloat("_PathLength", distance);
        }
        if (distance <= 1)
        {
            distanceText += " m remaining";
        }
        else
        {
            distanceText += " m remaining";
        }
        remainingDistance.text = distanceText;
    }

    // Show arrival state, is called from NavigationController.cs
    public void ShowArrivedState()
    {
        ShowNavigationUIElements(false);
        ToastManager.Instance.ShowAlert("You arrived at the destination!");
    }
}
