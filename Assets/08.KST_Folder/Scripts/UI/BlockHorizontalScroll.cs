using DanielLochner.Assets.SimpleScrollSnap;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BlockHorizontalScroll : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private ScrollRect _verticalScroll;
    [SerializeField] private SimpleScrollSnap _simpleScroll;
    [SerializeField] private float dragPX = 30f;
    private Vector2 _dragStart;
    private bool _isDragHorizontal = false;

    /// <summary>
    /// 드래그 시작 시 호출되는 로직
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        //드래그 시작 위치 저장
        _dragStart = eventData.position;
        //드래그 시작 마다 horizontal 여부 초기화
        _isDragHorizontal = false;
    }

    /// <summary>
    /// 드래그 중일 시 호출되는 로직
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        //현재 위치 ~ 드래그 시작 지점까지의 이동 벡터 거리 구하기
        Vector2 length = eventData.position - _dragStart;

        //가로 이동량(x축)이 세로 이동량(y축) 보다 클 경우
        if (!_isDragHorizontal && Mathf.Abs(length.x) > Mathf.Abs(length.y))
        {
            //가로 스크롤로 판정
            _isDragHorizontal = true;
            //세로 스크롤 막기.
            _verticalScroll.enabled = false;
        }
    }

    /// <summary>
    /// 드래그 끝날 때 호출
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        //종료 지점 ~ 시작 지점 까지의 이동량 계산
        Vector2 totalDrag = eventData.position - _dragStart;
        //가로 스크롤일 경우
        if (_isDragHorizontal)
        {
            //일정 거리 이상 드래그 완료 시
            if (Mathf.Abs(totalDrag.x) > dragPX)
            {
                //방향에 따라 좌우 결정 simple 스크롤 움직이는 패널 결정
                if (totalDrag.x < 0)
                    _simpleScroll.GoToNextPanel();
                else
                    _simpleScroll.GoToPreviousPanel();
            }
        }
        //드래그 연산 종료 됨으로 세로 스크롤 재활성화
        _verticalScroll.enabled = true;
    }
}
