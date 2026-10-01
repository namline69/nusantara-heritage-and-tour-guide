using UnityEngine;

public class NavBarController : MonoBehaviour
{
    [SerializeField] NavBar[] items;
    [SerializeField] int defaultIndex = 0;

    void Start()
    {
        for (int i = 0; i < items.Length; i++)
        {
            int index = i;
            items[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => Select(index));
        }
        Select(defaultIndex);
    }

    void Select(int index)
    {
        for (int i = 0; i < items.Length; i++)
            items[i].SetActive(i == index);
    }
}