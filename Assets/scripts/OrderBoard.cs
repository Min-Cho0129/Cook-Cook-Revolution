using System.Collections.Generic;
using UnityEngine;

public class OrderBoard : MonoBehaviour
{
    [Header("Board")]
    public Transform boardSurface;          // the physical board transform
    public Vector2 boardWorldSize = new(1.6f, 1.0f); // width x height in world units

    [Header("Ticket Prefab")]
    public GameObject ticketPrefab;
    public Vector2 ticketSize = new(0.18f, 0.24f);   // width x height in world units

    [Header("Grid")]
    public int columns = 4;
    public int rows = 2;
    public Vector2 padding = new(0.02f, 0.02f);       // gap between tickets

    [Header("Placement")]
    public float surfaceOffset = 0.005f;              // how far tickets float off the board

    [Header("Spawning")]
    public float minSpawnInterval = 3f;
    public float maxSpawnInterval = 6f;
    private float spawnTimer = 0;

    // runtime
    readonly List<GameObject> activeTickets = new();
    readonly List<Vector3> gridSlots = new();         // local positions of each slot

    private void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            spawnTimer = Random.Range(minSpawnInterval, maxSpawnInterval);
            if (RecipeManager.Instance != null)
            {
                //print("Spawning new order");
                RecipeManager.Instance.SpawnOrder();
            }
        }
    }

    // ── public API ────────────────────────────────────────────────

    public void PlaceTicket(Order order)
    {
        Recipe recipe = order.recipe;
        //Debug.Log($"OrderBoard: PlaceTicket called for {recipe.recipeName}, slots: {gridSlots.Count}, free: {FindFreeSlot()}");

        int slot = FindFreeSlot();
        if (slot < 0)
        {
            Debug.LogWarning("OrderBoard: no free slots");
            return;
        }

        Vector3 localPos = gridSlots[slot];
        Vector3 worldPos = boardSurface.TransformPoint(localPos);
        Quaternion worldRot = boardSurface.rotation;   // flush with board face

        GameObject ticket = Instantiate(ticketPrefab, worldPos, worldRot, boardSurface);

        var ui = ticket.GetComponent<OrderTicket>();
        if (ui != null){
            ui.setOrder(order);
            ui.SetScale(ComputeTicketScale(ticket));
        }

        // store slot index on the ticket so we can free it later
        var tracker = ticket.AddComponent<TicketSlotTracker>();
        tracker.slotIndex = slot;

        activeTickets.Add(ticket);
    }

    public void CompleteTicket(Order order)
    {
        for (int i = activeTickets.Count - 1; i >= 0; i--)
        {
            var ui = activeTickets[i]?.GetComponent<OrderTicket>();
            if (ui != null && ui.order == order)  // match by order reference
            {
                ui.CompleteTicket();
                activeTickets.RemoveAt(i);
                return;
            }
        }
    }

    public void ExpireTicket(Order order)
    {
        for (int i = activeTickets.Count - 1; i >= 0; i--)
        {
            var ui = activeTickets[i]?.GetComponent<OrderTicket>();
            if (ui != null && ui.order == order)  // match by order reference
            {
                ui.ExpireTicket();
                activeTickets.RemoveAt(i);
                return;
            }
        }
    }

    public void RemoveTicket(Order order)
    {
        for (int i = activeTickets.Count - 1; i >= 0; i--)
        {
            var ui = activeTickets[i]?.GetComponent<OrderTicket>();
            if (ui != null && ui.order == order)  // match by order reference
            {
                Destroy(activeTickets[i]);
                activeTickets.RemoveAt(i);
                return;
            }
        }
    }

    public void ClearAll()
    {
        foreach (var t in activeTickets) if (t != null) Destroy(t);
        activeTickets.Clear();
    }

    // ── lifecycle ─────────────────────────────────────────────────

    void Start()
    {
        BuildGrid();

        if (RecipeManager.Instance != null)
        {
            RecipeManager.Instance.OnOrderAdded += PlaceTicket;
            RecipeManager.Instance.OnOrderCompleted += CompleteTicket;
            RecipeManager.Instance.OnOrderExpired += ExpireTicket;
        }
        else
        {
            Debug.LogError("OrderBoard: RecipeManager.Instance is null in Start");
        }
    }

    void OnDestroy()
    {
        if (RecipeManager.Instance != null)
        {
            RecipeManager.Instance.OnOrderAdded -= PlaceTicket;
            RecipeManager.Instance.OnOrderCompleted -= CompleteTicket;
            RecipeManager.Instance.OnOrderExpired -= ExpireTicket;
        }
    }

    // ── grid ──────────────────────────────────────────────────────

    void BuildGrid()
    {
        gridSlots.Clear();

        // total space taken by tickets + padding
        float totalW = columns * ticketSize.x + (columns - 1) * padding.x;
        float totalH = rows    * ticketSize.y + (rows    - 1) * padding.y;

        // top-left corner in local space (board local: right = +X, up = +Y, out = -Z)
        float startX = -totalW / 2f + ticketSize.x / 2f;
        float startY =  totalH / 2f - ticketSize.y / 2f;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                float x = startX + c * (ticketSize.x + padding.x);
                float y = startY - r * (ticketSize.y + padding.y);
                gridSlots.Add(new Vector3(x, y, -surfaceOffset));
            }
        }
    }

    int FindFreeSlot()
    {
        bool[] occupied = new bool[gridSlots.Count];
        foreach (var t in activeTickets)
        {
            if (t == null) continue;
            var tracker = t.GetComponent<TicketSlotTracker>();
            if (tracker != null && tracker.slotIndex < occupied.Length)
                occupied[tracker.slotIndex] = true;
        }
        for (int i = 0; i < occupied.Length; i++)
            if (!occupied[i]) return i;
        return -1;
    }

    // ── scale ─────────────────────────────────────────────────────

    Vector3 ComputeTicketScale(GameObject ticket)
    {
        var r = ticket.GetComponentInChildren<Renderer>();
        if (r == null) return ticket.transform.localScale;

        Vector3 nat = r.bounds.size;
        if (nat.x < 0.0001f || nat.y < 0.0001f) return ticket.transform.localScale;

        float sx = ticketSize.x / nat.x;
        float sy = ticketSize.y / nat.y;
        float uniformScale = (sx + sy) / 2f;  // average of x and y scale factor

        return new Vector3(sx, sy, uniformScale);
    }

    // ── gizmos ────────────────────────────────────────────────────

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (boardSurface == null) return;

        // board outline
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.3f);
        DrawLocalRect(Vector3.zero, boardWorldSize);

        // rebuild grid preview without touching runtime list
        float totalW = columns * ticketSize.x + (columns - 1) * padding.x;
        float totalH = rows    * ticketSize.y + (rows    - 1) * padding.y;
        float startX = -totalW / 2f + ticketSize.x / 2f;
        float startY =  totalH / 2f - ticketSize.y / 2f;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                float x = startX + c * (ticketSize.x + padding.x);
                float y = startY - r * (ticketSize.y + padding.y);
                Vector3 localCenter = new Vector3(x, y, -surfaceOffset);

                // slot rect
                Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.25f);
                DrawLocalRect(localCenter, ticketSize);

                // slot border
                Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.8f);
                DrawLocalRectOutline(localCenter, ticketSize);

                // slot number
                Vector3 worldCenter = boardSurface.TransformPoint(localCenter);
                UnityEditor.Handles.Label(worldCenter,
                    $"{r * columns + c}",
                    new GUIStyle { normal = { textColor = Color.yellow }, fontSize = 10 });
            }
        }

        // board outline (solid border on top)
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.9f);
        DrawLocalRectOutline(Vector3.zero, boardWorldSize);
    }

    void DrawLocalRect(Vector3 localCenter, Vector2 size)
    {
        Vector3 c = boardSurface.TransformPoint(localCenter);
        Vector3 right = boardSurface.right   * size.x * 0.5f;
        Vector3 up    = boardSurface.up      * size.y * 0.5f;

        Vector3 tl = c - right + up;
        Vector3 tr = c + right + up;
        Vector3 bl = c - right - up;
        Vector3 br = c + right - up;

        // filled quad via two triangles using DrawLine as outline — use mesh for fill
        Gizmos.DrawLine(tl, tr);
        Gizmos.DrawLine(tr, br);
        Gizmos.DrawLine(br, bl);
        Gizmos.DrawLine(bl, tl);
    }

    void DrawLocalRectOutline(Vector3 localCenter, Vector2 size)
    {
        DrawLocalRect(localCenter, size); // same thing, color set by caller
    }
#endif
}

// small marker component so tickets know which slot they occupy
public class TicketSlotTracker : MonoBehaviour
{
    public int slotIndex;
}