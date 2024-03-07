using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UITokenBetStack : MonoBehaviour
{
    [SerializeField]
    private GameObject m_tokenPrefab;
    [SerializeField]
    private Transform m_tokensContainer;

    private Dictionary<string, List<Image>> m_tokens = new Dictionary<string, List<Image>>();

    private List<Image> m_tokenPool = new List<Image>();

    public void AddToken(UIToken uiToken)
    {
        Image tokenImage = TakeTokenFromPool();
        //tokenImage.color = uiToken.Token.Color;
        tokenImage.gameObject.SetActive(true);

        if(!m_tokens.ContainsKey(uiToken.Id))
        {
            m_tokens[uiToken.Id] = new List<Image>();
        }

        var list = m_tokens[uiToken.Id];
        list.Add(tokenImage);
    }

    public void RemoveToken(UIToken uiToken)
    {
        var list = m_tokens[uiToken.Id];
        var tokenImage = list.LastOrDefault();
        list.Remove(tokenImage);

        ReleaseToPool(tokenImage);
    }

    public void ClearStack()
    {
        foreach(string key in m_tokens.Keys)
        {
            var list = m_tokens[key];
            foreach(Image image in list)
            {
                ReleaseToPool(image);
            }
            list.Clear();
        }
    }

    private Image TakeTokenFromPool()
    {
        if(m_tokenPool.Count == 0)
        {
            var go = Instantiate(m_tokenPrefab, m_tokensContainer);
            return go.GetComponent<Image>();
        }

        var token = m_tokenPool[0];
        m_tokenPool.RemoveAt(0);
        return token;
    }

    private void ReleaseToPool(Image image)
    {
        image.gameObject.SetActive(false);
        image.transform.SetAsLastSibling();
        m_tokenPool.Add(image);
    }

}
