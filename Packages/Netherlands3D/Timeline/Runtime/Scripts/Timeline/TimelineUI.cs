using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using SLIDDES.UI;
using TMPro;
using System.Globalization;
using UnityEngine.EventSystems;
using Netherlands3D.Events;
using UnityEngine.InputSystem;

namespace Netherlands3D.Timeline
{
    /// <summary>
    /// Main script for time line UI
    /// </summary>
    [AddComponentMenu("Netherlands3D/Timeline/TimelineUI")]
    public class TimelineUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        /// <summary>
        /// The width of the parent rect
        /// </summary>
        public float TimeBarParentWidth { get { return timeBarParent.rect.width; } }

        [Header("Scriptable Objects")]
        [Tooltip("The scriptable so data")]
        public TimelineData timelineData;
        [Tooltip("Event callback when the currentDate is changed")]
        public DateTimeEvent dateTimeEvent;

        [Header("Values")]
        [SerializeField] private float mouseSensitivity = 10;

        [Header("UI Components")]
        public RectTransform timeBarParent;
        [Tooltip("The 3 timebar script components")]
        public TimeBar[] timeBars = new TimeBar[3];
        [Tooltip("The input field of the current date")]
        public TMP_InputField inputFieldCurrentDate;
        [Tooltip("For changing the playback speed")]
        public TMP_InputField inputFieldPlaySpeed;

        [Header("Timeline Components")]
        [SerializeField] private GameObject prefabTimePeriodsUILayer;
        [SerializeField] private GameObject prefabTimePeriodUI;
        [SerializeField] private GameObject prefabLayerUI;
        [SerializeField] private Transform parentTimePeriodsLayers;
        [SerializeField] private Transform parentLayersUI;
        [SerializeField] private ScrollRect scrollRectTimePeriodLayers;
        [SerializeField] private ScrollRect scrollRectLayers;
        public TimelinePlayback playback;

        [Header("Time Scrubber Components")]
        public TimeScrubber timeScrubber;

        /// <summary>
        /// Unity Event that gets triggerd when the currentDate is changed
        /// </summary>
        [HideInInspector] public UnityEvent<DateTime> onCurrentDateChange = new UnityEvent<DateTime>();
        /// <summary>
        /// Unity Event that gets triggerd when a category is toggled
        /// </summary>
        /// <remarks><categoryName, setActive></remarks>
        [HideInInspector] public UnityEvent<string, bool> onCategoryToggle = new UnityEvent<string, bool>();

        /// <summary>
        /// The current time line date
        /// </summary>
        /// <seealso cref="SetCurrentDate(DateTime)" "For setting currentDate"/>
        public DateTime CurrentDate 
        { 
            get { return currentDate; } 
            set
            {
                previousCurrentDate = currentDate;
                currentDate = value;
                onCurrentDateChange?.Invoke(currentDate);
                if(dateTimeEvent != null) dateTimeEvent.InvokeStarted(currentDate);
            }
        }

        /// <summary>
        /// If the currentDate is set at the start
        /// </summary>
        private bool currentDateIsSet;
        /// <summary>
        /// Is the user dragging its mouse on the time bar?
        /// </summary>
        private bool mouseIsDragging;
        /// <summary>
        /// Is the mouse on the ui?
        /// </summary>
        private bool mouseIsOn;
        /// <summary>
        /// Has the pointer event data for the scroll rects been send?
        /// </summary>
        private bool hasSendPointerEventData;
        /// <summary>
        /// For skipping through timePeriods
        /// </summary>
        private int skipIndex = -1;
        /// <summary>
        /// Array int holding the order of the indexes of timeBars in which they appear/move
        /// </summary>
        private int[] barIndexes;

        private float scrollSpeed;
        /// <summary>
        /// The time unit used for the timeline
        /// </summary>
        private TimeUnit.Unit timeUnit = TimeUnit.Unit.year;
        /// <summary>
        /// The position of the mouse when left input was pressed
        /// </summary>
        private Vector3 mouseDownPosition;
        /// <summary>
        /// The current timeline date
        /// </summary>
        private DateTime currentDate;
        /// <summary>
        /// The previous currentDate
        /// </summary>
        private DateTime previousCurrentDate;
        /// <summary>
        /// The most visable date left
        /// </summary>
        private DateTime visibleDateLeft;
        /// <summary>
        /// The most visable date right
        /// </summary>
        private DateTime visibleDateRight;        
        /// <summary>
        /// List of all categories scripts
        /// </summary>
        private List<LayerUI> categories = new List<LayerUI>();
        /// <summary>
        /// String of each event layer with as key category name
        /// </summary>
        private Dictionary<string, TimePeriodsLayer> eventLayers = new Dictionary<string, TimePeriodsLayer>();
        /// <summary>
        /// Dictionary containing the active eventUI scripts with corresponding data.data index id
        /// </summary>
        /// <remarks><data.data[i], EventUI></remarks>
        private Dictionary<int, TimePeriodUI> visibleTimePeriodsUI = new Dictionary<int, TimePeriodUI>();
        /// <summary>
        /// Pointer event data that gets used for the scrollrects
        /// </summary>
        private PointerEventData pointerEventDataBeginDragHandler;
        /// <summary>
        /// Pointer event data that gets used for the scrollrects
        /// </summary>
        private PointerEventData pointerEventDataEndDragHandler;

        private void OnEnable()
        {
            timelineData.OnOrderTimePeriods.AddListener(LoadData);
        }

        private void OnDisable()
        {
            timelineData.OnOrderTimePeriods.RemoveListener(LoadData);
        }

        // Start is called before the first frame update
        void Start()
        {
            // Triggers LoadData
            timelineData.OrderTimePeriods();
            SetCurrentDate(new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0));
            currentDateIsSet = true;
        }

        private void Update()
        {
            OnPointerStay();
        }

        #region Timeline

        /// <summary>
        /// Clear the timeline data
        /// </summary>
        public void ClearData()
        {
            timelineData.ClearData();
        }

        /// <summary>
        /// Get the closest bar based on the local x position
        /// </summary>
        /// <returns></returns>
        public TimeBar GetClosestBar(float posX)
        {
            return timeBars.OrderBy(x => Math.Abs(posX - x.transform.localPosition.x)).FirstOrDefault();
        }

        /// <summary>
        /// Get the focused bar
        /// </summary>
        /// <returns></returns>
        public TimeBar GetFocusedBar()
        {
            // Based on time bar which is closest to position.x 0
            return timeBars.OrderBy(x => Math.Abs(0 - x.transform.localPosition.x)).FirstOrDefault();
        }

        /// <summary>
        /// Set a new timeline data object
        /// </summary>
        /// <param name="timelineData">The timeline data object</param>
        public void SetData(TimelineData timelineData)
        {
            this.timelineData = timelineData;
            this.timelineData.OrderTimePeriods();
            LoadData();
        }

        /// <summary>
        /// Load the TimelineData in the UI
        /// </summary>
        public void LoadData()
        {
            if(timelineData == null)
            {
                Debug.LogError("[TimelineUI] Data not assigned");
                return;
            }
            Debug.Log("[TimelineUI] Load Data");

            // Clear old
            foreach(Transform item in parentTimePeriodsLayers.transform) Destroy(item.gameObject);
            foreach(Transform item in parentLayersUI.transform) Destroy(item.gameObject);
            categories.Clear();
            eventLayers.Clear();
            visibleTimePeriodsUI.Clear();

            // Create each time period layer & layer
            string[] keys = timelineData.sortedTimePeriods.Keys.ToArray();
            foreach(string item in keys)
            {
                TimePeriodsLayer e = Instantiate(prefabTimePeriodsUILayer, parentTimePeriodsLayers).GetComponent<TimePeriodsLayer>();
                eventLayers.Add(item, e);

                LayerUI c = Instantiate(prefabLayerUI, parentLayersUI).GetComponent<LayerUI>();
                c.Initialize(item, e, this);
                categories.Add(c);
            }

            if(currentDateIsSet) SetCurrentDate(CurrentDate);
        }

        /// <summary>
        /// When the input field receives a new date
        /// </summary>
        public void OnInputFieldCurrentDateChanged()
        {
            // Try to parse the new date
            DateTime result;
            if(DateTime.TryParseExact(inputFieldCurrentDate.text, "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                timeUnit = TimeUnit.Unit.year;
                SetCurrentDate(result);
            }
            else if(DateTime.TryParseExact(inputFieldCurrentDate.text, "MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                timeUnit = TimeUnit.Unit.month;
                SetCurrentDate(result);
            }
            else if(DateTime.TryParseExact(inputFieldCurrentDate.text, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                timeUnit = TimeUnit.Unit.day;
                SetCurrentDate(result);
            }
            else
            {
                UpdateCurrentDateVisual();
            }
            timeScrubber.StoppedUsing();
        }

        /// <summary>
        /// Scroll the time bar horizontally by a amount
        /// </summary>
        /// <param name="scrollAmount">The amount to scroll the time bar</param>
        /// <param name="resetTimeScrubber">Should the time scrubber be resetted when scrolling the time bar?</param>
        public void ScrollTimeBar(float scrollAmount, bool resetTimeScrubber = true)
        {
            // Check scroll direction
            if(scrollAmount < 0)
            {
                // Scrolling left
                int leaderIndex = barIndexes[0];

                // Update the first in line panel
                timeBars[leaderIndex].rectTransform.localPosition += Vector3.right * scrollAmount;

                // If first in line is out of bounds set new first in line
                if(timeBars[leaderIndex].rectTransform.localPosition.x <= timeBarParent.localPosition.x + TimeBarParentWidth * -1)
                {
                    // Circular rotate array left
                    ArrayExtention.Rotate(barIndexes, -1);
                    leaderIndex = barIndexes[0];
                    // Update visuals of previous leader bar
                    timeBars[barIndexes.Last()].UpdateVisuals(timeBars[barIndexes[1]].StartDateTime, 2, timeUnit);
                }

                // Update remaining indexes positions
                for(int i = 1; i < barIndexes.Length; i++)
                {
                    int index = barIndexes[i];                    
                    timeBars[index].rectTransform.localPosition = new Vector3(timeBars[leaderIndex].rectTransform.localPosition.x + i * TimeBarParentWidth, 0, 0);
                }
            }
            else
            {
                // Scrolling right
                int leaderIndex = barIndexes[barIndexes.Length - 1];

                // Update the leader position
                timeBars[leaderIndex].rectTransform.localPosition += Vector3.right * scrollAmount;

                // If last in line is out of bounds set new last in line
                if(timeBars[leaderIndex].rectTransform.localPosition.x >= timeBarParent.localPosition.x + TimeBarParentWidth)
                {
                    // Circular rotate array right
                    ArrayExtention.Rotate(barIndexes, 1);
                    leaderIndex = barIndexes[barIndexes.Length - 1];
                    // Update visuals of previous leader bar
                    timeBars[barIndexes.First()].UpdateVisuals(timeBars[barIndexes[1]].StartDateTime, 0, timeUnit);
                }

                // Update remaining indexes positions
                for(int i = 0; i < barIndexes.Length - 1; i++)
                {
                    int index = barIndexes[i];
                    timeBars[index].rectTransform.localPosition = new Vector3(timeBars[leaderIndex].rectTransform.localPosition.x - (2 - i) * TimeBarParentWidth, 0, 0);
                }
            }

            // Get currentDate from middle bar
            CurrentDate = GetFocusedBar().GetCurrentDateTime(timeScrubber.transform.localPosition.x);
            UpdateCurrentDateVisual();
            UpdateVisableDateRange();

            // Reset timescrubber
            if(resetTimeScrubber) timeScrubber.StoppedUsing();
        }

        /// <summary>
        /// Set the current date of the time line and go to that date
        /// </summary>
        public void SetCurrentDate(DateTime newDate)
        {
            CurrentDate = newDate;

            UpdateTimeBars();
            SetFocusedBarPosition(GetFocusedBar().GetDatePosition(CurrentDate, timeUnit));
            UpdateCurrentDateVisual();
            UpdateVisableDateRange();
        }

        /// <summary>
        /// Set the current date without a notify
        /// </summary>
        /// <param name="newDate">The new currentDate</param>
        public void SetCurrentDateNoNotify(DateTime newDate)
        {
            CurrentDate = newDate;
            UpdateCurrentDateVisual();
        }

        /// <summary>
        /// Set the timeline currentDateTimeUnit
        /// </summary>
        /// <param name="valueToAdd">The value to add to currentDateTimeUnit</param>
        public void SetTimeUnit(int valueToAdd)
        {
            TimeUnit.ChangeUnit(ref timeUnit, valueToAdd);
            // If timeUnit is 5x/10x round the current date to the x number (2022 becomes 2020, 2026 becomes 2025 with a 5x zoom)
            DateTime dt = CurrentDate;
            if(timeUnit < TimeUnit.Unit.year)
            {
                int year = dt.Year;
                switch(timeUnit)
                {
                    case TimeUnit.Unit.year10:
                        year = ((int)Math.Round(year / 10.0)) * 10;
                        break;
                    case TimeUnit.Unit.year5:
                        year = ((int)Math.Round(year / 5.0)) * 5;
                        break;
                    default:
                        break;
                }
                dt = new DateTime(year, dt.Month, dt.Day,0,0,0);
            }            
            SetCurrentDate(dt);
            UpdateCurrentDateVisual();
            timeScrubber.StoppedUsing();
        }

        /// <summary>
        /// Skip the timeline to the next time period
        /// </summary>
        /// <param name="forward"></param>
        public void SkipToNextTimePeriod(bool forward)
        {
            // This stuff feels like spagetti but it works
            List<TimePeriod> timePeriods = timelineData.allTimePeriods;
            timePeriods.Sort((x, y) => x.CompareTo(y));
            // Get the next time period
            if(skipIndex == -1)
            {
                var closestTimePeriod = ArrayExtention.MinBy(timelineData.allTimePeriods, x => Math.Abs((x.startDate - CurrentDate).Ticks));
                skipIndex = timePeriods.IndexOf(closestTimePeriod);
            }

            // Forward / backwards
            skipIndex += forward ? 1 : -1;
            if(skipIndex >= timePeriods.Count)
                skipIndex = 0;
            if(skipIndex < 0) skipIndex = timePeriods.Count - 1;

            // Skip
            SetCurrentDate(timePeriods[skipIndex].startDate);
        }


        /// <summary>
        /// Get the local x position of a date from the timeBars
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns>The rect left or right value<returns>
        private float EventUIGetPosX(DateTime dateTime, bool isRight)
        {
            // Correct dateTime
            dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0);

            //Loop through timebars to check if the date is available
            for(int i = 0; i < timeBars.Length; i++)
            {
                float value = timeBars[i].GetDatePosition(dateTime, timeUnit);
                if(value == 0.123f) continue; // Not found
                else
                {
                    // Found local x value of date in timebar
                    // from the selected time bar get its local x position
                    float timebarPosX = timeBars[i].transform.localPosition.x;

                    // If timeBarPosX and its value pos is bigger then the screen width/2 then it is out of bounds
                    float boundsValue;
                    if(timebarPosX < 0)
                    {
                        boundsValue = timebarPosX + value * -1;
                    }
                    else
                    {
                        boundsValue = timebarPosX - value;
                    }
                    //print("bounds: " + dateTime + " " + i + " : " + boundsValue + " timebarPosX " + timebarPosX + " value " + value);
                    if(boundsValue > TimeBarParentWidth / 2 || boundsValue < -TimeBarParentWidth / 2)
                    {
                        //Debug.LogWarning("out bounds");
                        return 0;
                    }

                    // Negative or positive
                    if(timebarPosX < 0)
                    {
                        // Time bar is left from 0
                        // deduct value from timebarPosX (get diff from 0 point)
                        float midpointInBar = Mathf.Abs(timebarPosX) + value;
                        // From center deduct value
                        if(!isRight)
                        {
                            return (TimeBarParentWidth / 2) - midpointInBar;

                        }
                        return (TimeBarParentWidth / 2) + midpointInBar;
                    }
                    else
                    {
                        float midpointInBar = timebarPosX - value;
                        // From center deduct value
                        if(!isRight)
                        {
                            return (TimeBarParentWidth / 2) + midpointInBar;

                        }
                        return (TimeBarParentWidth / 2) - midpointInBar;
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// Get all the datetimes stored in the time bars
        /// </summary>
        /// <returns></returns>
        private DateTime[] GetTimeBarDateTimes()
        {
            List<DateTime> dateTimes = new List<DateTime>();
            for(int i = 0; i < timeBars.Length; i++)
            {
                dateTimes.AddRange(timeBars[i].dateTimePositions.Values.ToList());
            }
            return dateTimes.ToArray();
        }

        /// <summary>
        /// Check if the event is visible in the visable date range
        /// </summary>
        /// <param name="dEvent">The even to check</param>
        private bool IsTimePeriodVisible(TimePeriod dEvent)
        {
            return  dEvent.startDate <= visibleDateLeft && dEvent.endDate >= visibleDateRight ||                                            // 0---[-------]---0
                    dEvent.startDate <= visibleDateLeft && dEvent.endDate <= visibleDateRight && dEvent.endDate >= visibleDateLeft ||       // 0---[---0   ]
                    dEvent.startDate >= visibleDateLeft && dEvent.startDate <= visibleDateRight && dEvent.endDate >= visibleDateRight ||    //     [   0---]---0
                    dEvent.startDate >= visibleDateLeft && dEvent.endDate <= visibleDateRight;                                              //     [0-----0]         
        }

        /// <summary>
        /// Set the focused bar local position.
        /// </summary>
        /// <example>
        /// When setting the currentDate the bar will not center itself on the set date, thats where this funtion comes in place to move the local x positions
        /// </example>
        /// <param name="localPosX">The local x position to set the bar to</param>
        private void SetFocusedBarPosition(float localPosX)
        {
            // Middle bar
            TimeBar t1 = timeBars[barIndexes[1]]; 
            t1.transform.localPosition = new Vector3(localPosX, t1.transform.localPosition.y, t1.transform.localPosition.z);
            // Left bar
            TimeBar t0 = timeBars[barIndexes[0]];
            t0.transform.localPosition = new Vector3(localPosX - TimeBarParentWidth, t0.transform.localPosition.y, t0.transform.localPosition.z);
            // Right bar
            TimeBar t2 = timeBars[barIndexes[2]];
            t2.transform.localPosition = new Vector3(localPosX + TimeBarParentWidth, t2.transform.localPosition.y, t2.transform.localPosition.z);
        }
        
        /// <summary>
        /// Updates the current date inputfield text to currentDate datetime value
        /// </summary>
        private void UpdateCurrentDateVisual()
        {
            inputFieldCurrentDate.text = CurrentDate.ToString(TimeUnit.GetUnitFullString(timeUnit));
        }

        /// <summary>
        /// Based on what is visable show the events
        /// </summary>
        private void UpdateTimePeriods()
        {
            // Based on whats visable, show corresponding time periods
            // First loop trough already existing time periods and check if they are still visable or need to be removed
            int[] keys = visibleTimePeriodsUI.Keys.ToArray();
            int k;
            for(int i = keys.Length - 1; i >= 0; i--) // Loop backwards
            {
                k = keys[i];
                if(!IsTimePeriodVisible(visibleTimePeriodsUI[k].timePeriod))
                {
                    // Remove
                    visibleTimePeriodsUI[k].Remove();
                    visibleTimePeriodsUI.Remove(k);
                }
            }

            // Loop through each time period
            TimePeriod timePeriod;
            for(int i = 0; i < timelineData.allTimePeriods.Count; i++)
            {
                // If time periods is already in visibleEventsUI skip it since it is already checked
                if(visibleTimePeriodsUI.ContainsKey(i)) continue;

                // Check if time period is visable and if so add it
                timePeriod = timelineData.allTimePeriods[i];
                if(IsTimePeriodVisible(timePeriod))
                {
                    // Time period is visable, show it & add to time periods layer
                    visibleTimePeriodsUI.Add(i, eventLayers[timePeriod.layer].AddTimePeriod(timePeriod, prefabTimePeriodUI));
                }
            }

            // Now update each visible time period
            foreach(var item in visibleTimePeriodsUI.Values)
            {
                // Get left and right x positions correlating to the bar
                // but check if no *X timeunit is used
                float xL, xR;
                if(timeUnit == TimeUnit.Unit.year5 || timeUnit == TimeUnit.Unit.year10)
                {
                    // *X used (as in 5 years, 10 years etc)
                    // Get the closest year of the timebar & stick it 2 that
                    DateTime dL = TimeUnit.GetClosestDateTime(item.timePeriod.startDate, GetTimeBarDateTimes());
                    DateTime dR = TimeUnit.GetClosestDateTime(item.timePeriod.endDate, GetTimeBarDateTimes());
                    xL = EventUIGetPosX(dL, false);
                    xR = EventUIGetPosX(dR, true);
                }
                else
                {
                    // Normal
                    xL = EventUIGetPosX(item.timePeriod.startDate, false);
                    //Debug.LogWarning(visableDateLeft + " XL " + xL);
                    xR = EventUIGetPosX(item.timePeriod.endDate, true);
                    //Debug.LogWarning(visableDateRight + " XR " + xR);
                }
                item.UpdateUI(xL, xR); // some wierd bug setting it off by some pixels, so applied ductapefix of -16

                // Check the time period events
                // Current Time Enter
                if(TimeUnit.DateTimeInRange(currentDate, item.timePeriod.startDate, item.timePeriod.endDate) &&
                    !TimeUnit.DateTimeInRange(previousCurrentDate, item.timePeriod.startDate, item.timePeriod.endDate))
                {
                    item.timePeriod.eventCurrentTimeEnter?.Invoke();
                }
                // Current Time Exit
                else if(TimeUnit.DateTimeInRange(previousCurrentDate, item.timePeriod.startDate, item.timePeriod.endDate) &&
                    !TimeUnit.DateTimeInRange(currentDate, item.timePeriod.startDate, item.timePeriod.endDate))
                {
                    item.timePeriod.eventCurrentTimeExit?.Invoke();
                }
            }
        }

        /// <summary>
        /// Setup the time bars
        /// </summary>
        private void UpdateTimeBars()
        {
            // Position the time bars correctly
            // Hardcoded 3 time bars, as there is no need for more than 3
            timeBars[0].rectTransform.SetRect(0, 0, -timeBarParent.rect.width, timeBarParent.rect.width);
            timeBars[1].rectTransform.SetRect(0, 0, 0, 0);
            timeBars[2].rectTransform.SetRect(0, 0, timeBarParent.rect.width, -timeBarParent.rect.width);
            barIndexes = new int[] { 0, 1, 2 };

            // Time bar visual
            for(int i = 0; i < 3; i++)
            {
                timeBars[i].UpdateVisuals(CurrentDate, i, timeUnit);
            }
        }

        /// <summary>
        /// Get the visable date range the user can see
        /// </summary>
        /// <remarks>
        /// We need the visable date range in order to know what loaded data we need to display
        /// </remarks>
        private void UpdateVisableDateRange()
        {
            int datesToPlace = (int)((TimeBarParentWidth / TimeBar.PixelDistanceDates) / 2);
            // based on timeUnit & current date, get the most left and right date
            visibleDateLeft = TimeUnit.GetVisibleDateLeftRight(true, CurrentDate, timeUnit, datesToPlace);
            visibleDateRight = TimeUnit.GetVisibleDateLeftRight(false, CurrentDate, timeUnit, datesToPlace);
            // Correct dates
            visibleDateLeft = new DateTime(visibleDateLeft.Year, visibleDateLeft.Month, visibleDateLeft.Day, 0, 0, 0);
            visibleDateRight = new DateTime(visibleDateRight.Year, visibleDateRight.Month, visibleDateRight.Day, 0, 0, 0);
            //print(visableDateLeft + " - " + visableDateRight);

            UpdateTimePeriods();
        }

        #endregion Timeline

        #region Mouse Interaction

        /// <summary>
        /// User presses mouse down on time bar
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerDown(PointerEventData eventData)
        {
            mouseIsDragging = true;
            mouseDownPosition = Input.mousePosition;
        }

        /// <summary>
        /// The mouse is on the ui
        /// </summary>
        public void OnPointerStay()
        {
            if(mouseIsDragging)
            {
                var mousePosition = Mouse.current.position.ReadValue();

                float x = Mathf.Abs(mouseDownPosition.x - mousePosition.x);
                float y = Mathf.Abs(mouseDownPosition.y - mousePosition.y);
                // Based on direction, scroll horizontal or vertical (& minimum distance needed)
                if(Math.Abs(x - y) >= 3 && x >= y) //TODO mouse x/y overflow needs to be smooth instead of having to mouseup/down to switch
                {
                    int dirX = mouseDownPosition.x < mousePosition.x ? 1 : -1;
                    scrollRectTimePeriodLayers.enabled = false;
                    scrollRectLayers.enabled = false;
                    ScrollTimeBar(Vector3.Distance(mouseDownPosition, mousePosition) * dirX * mouseSensitivity * UnityEngine.Time.deltaTime);
                    playback.PlayScroll(false);
                }
                else
                {
                    scrollRectTimePeriodLayers.enabled = true;
                    scrollRectLayers.enabled = true;
                    if(!hasSendPointerEventData && pointerEventDataBeginDragHandler != null)
                    {
                        scrollRectTimePeriodLayers.OnBeginDrag(pointerEventDataBeginDragHandler);
                        scrollRectLayers.OnBeginDrag(pointerEventDataBeginDragHandler);
                        hasSendPointerEventData = true;
                    }
                }
            }

            if(mouseIsOn)
            {
                var scrollY = Mouse.current.scroll.ReadValue().y;
                if (scrollY < 0)
                {
                    // Up
                    SetTimeUnit(-1);
                }
                else if(scrollY > 0)
                {
                    // Down
                    SetTimeUnit(1);
                }
            }
        }

        /// <summary>
        /// User presses mouse up on time bar
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerUp(PointerEventData eventData)
        {
            mouseIsDragging = false;
            scrollRectTimePeriodLayers.enabled = true;
            scrollRectLayers.enabled = true;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            mouseIsOn = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            mouseIsOn = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if(scrollRectTimePeriodLayers.enabled)
            {
                scrollRectTimePeriodLayers.OnDrag(eventData);
                scrollRectLayers.OnDrag(eventData);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            hasSendPointerEventData = false;
            pointerEventDataBeginDragHandler = eventData;            
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            pointerEventDataEndDragHandler = eventData;
            scrollRectTimePeriodLayers.OnEndDrag(eventData);
            scrollRectLayers.OnEndDrag(eventData);
        }

        #endregion
    }
}
