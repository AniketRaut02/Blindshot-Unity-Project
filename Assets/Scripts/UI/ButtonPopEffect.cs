using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonPopEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float speed = 10f;

    private Vector3 targetScale;

    private void Start()
    {
        targetScale = Vector3.one * normalScale;
        transform.localScale = targetScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            speed * Time.deltaTime
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PopUp();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PopDown();
    }

    // Function you can call manually as well
    public void PopUp()
    {
        targetScale = Vector3.one * hoverScale;
    }
    public void PopDown()
    {
        targetScale = Vector3.one * normalScale;
    }
}