using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace SimplePaint

{
    public partial class Form1 : Form
    {
        // 1. 사용할 도형 타입 정의
        enum ToolType
        {
            Line,
            Rectangle,
            Circle
        }

        // 2. 그리기 관련 멤버 변수
        private Bitmap canvasBitmap;           // 실제 그림이 저장되는 비트맵
        private Graphics canvasGraphics;       // 비트맵 위에 그리기 위한 객체

        private bool isDrawing = false;        // 현재 드래그 중인지 여부
        private Point startPoint;              // 드래그 시작점
        private Point endPoint;                // 드래그 끝점

        // 3. 도구 및 스타일 설정
        private ToolType currentTool = ToolType.Line;   // 현재 선택된 도형
        private Color currentColor = Color.Black;       // 현재 색상
        private int currentLineWidth = 2;               // 현재 선 두께

        public Form1()
        {
            InitializeComponent();

            // 1. PictureBox 크기와 동일한 비트맵(도화지) 생성
            canvasBitmap = new Bitmap(picCanvas.Width, picCanvas.Height);

            // 2. 비트맵에 그림을 그리기 위한 Graphics 객체 생성
            canvasGraphics = Graphics.FromImage(canvasBitmap);

            // 3. 캔버스 전체를 흰색으로 깨끗하게 채우기 (초기화)
            canvasGraphics.Clear(Color.White);

            // 4. 생성된 비트맵을 PictureBox의 Image로 연결하여 화면에 표시
            picCanvas.Image = canvasBitmap;

            // 1. 마우스 조작 이벤트 연결 (그리기 로직 제어)
            //picCanvas.MouseDown += PicCanvas_MouseDown;
            //picCanvas.MouseMove += PicCanvas_MouseMove;
            //picCanvas.MouseUp += PicCanvas_MouseUp;

            // 2. PictureBox 페인트 이벤트 연결 (화면 갱신 및 미리보기 처리)
            picCanvas.Paint += PicCanvas_Paint;

            // 3. 도형 선택 버튼 클릭 이벤트 연결
            btnLine.Click += btnLine_Click;
            btnRectangle.Click += btnRectangle_Click;
            btnCircle.Click += btnCircle_Click;

            // 1. 색상 선택 콤보박스 설정 및 이벤트 연결
            cmbColor.SelectedIndexChanged += cmbColor_SelectedIndexChanged;
            cmbColor.SelectedIndex = 0;      // 기본값 설정 (예: Black)

            // 2. 선 두께 트랙바 설정 및 이벤트 연결
            trbLineWidth.Minimum = 1;        // 최소 두께
            trbLineWidth.Maximum = 10;       // 최대 두께
            trbLineWidth.Value = 2;          // 초기 두께 값
            trbLineWidth.ValueChanged += trbLineWidth_ValueChanged;
        }

        private void picCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            isDrawing = true;             // 드래그시작
            startPoint = e.Location;      // 시작점저장
        }

        private void picCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            // 1. 그리기 상태가 아니면 로직 수행 안 함 (최적화)
            if (!isDrawing) return;

            // 2. 현재 마우스 좌표를 끝점(endPoint)으로 갱신
            endPoint = e.Location;

            // 3. PictureBox 무효화 (Paint 이벤트를 호출하여 미리보기 선을 그림)
            picCanvas.Invalidate();
        }

        private void picCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            // 1. 드래그 상태가 아니면 무시
            if (!isDrawing) return;

            // 2. 드래그 종료 상태로 변경 및 최종 위치 저장
            isDrawing = false;
            endPoint = e.Location;

            // 3. 실제 비트맵(도화지)에 도형 그리기 확정
            using (Pen pen = new Pen(currentColor, currentLineWidth))
            {
                // 선 끝을 부드럽게 처리 (옵션)
                pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;

                DrawShape(canvasGraphics, pen, startPoint, endPoint);
            }

            // 4. 최종 결과물을 화면에 반영
            picCanvas.Invalidate();
        }

        private void PicCanvas_Paint(object sender, PaintEventArgs e)
        {
            // 1. 드래그 중이 아닐 때는 미리보기를 그릴 필요 없음
            if (!isDrawing) return;

            // 2. 미리보기용 점선 펜 설정 (using으로 자원 해제 보장)
            using (Pen previewPen = new Pen(currentColor, currentLineWidth))
            {
                // 점선 스타일 적용
                previewPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                // 비트맵이 아닌 '이벤트 인자(e.Graphics)'에 그려서 화면에만 임시로 표시
                DrawShape(e.Graphics, previewPen, startPoint, endPoint);
            }
        }

        private void DrawShape(Graphics g, Pen pen, Point p1, Point p2)
        {
            // 1. 두 점을 기준으로 사각형 영역 계산 (사각형, 원 그리기용)
            Rectangle rect = GetRectangle(p1, p2);

            // 2. 선택된 도구 타입에 따라 그리기 수행
            switch (currentTool)
            {
                case ToolType.Line:
                    g.DrawLine(pen, p1, p2);
                    break;

                case ToolType.Rectangle:
                    g.DrawRectangle(pen, rect);
                    break;

                case ToolType.Circle:
                    g.DrawEllipse(pen, rect);
                    break;
            }
        }

        private Rectangle GetRectangle(Point p1, Point p2)
        {
            // Math.Min으로 시작점(좌측 상단)을 결정하고, 
            // Math.Abs로 가로/세로 길이를 절대값으로 계산합니다.
            return new Rectangle(
                Math.Min(p1.X, p2.X),    // 시작 X 좌표
                Math.Min(p1.Y, p2.Y),    // 시작 Y 좌표
                Math.Abs(p1.X - p2.X),   // 너비(Width)
                Math.Abs(p1.Y - p2.Y)    // 높이(Height)
            );
        }

        private void btnLine_Click(object sender, EventArgs e)
        {
            currentTool = ToolType.Line;
        }

        private void btnRectangle_Click(object sender, EventArgs e)
        {
            currentTool = ToolType.Rectangle;
        }

        private void btnCircle_Click(object sender, EventArgs e)
        {
            currentTool = ToolType.Circle;
        }

        private void cmbColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 선택된 인덱스에 따라 현재 펜 색상을 변경
            switch (cmbColor.SelectedIndex)
            {
                case 0: // 검정
                    currentColor = Color.Black;
                    break;

                case 1: // 빨강
                    currentColor = Color.Red;
                    break;

                case 2: // 파랑
                    currentColor = Color.Blue;
                    break;

                case 3: // 녹색
                    currentColor = Color.Green;
                    break;

                default: // 기본값: 검정
                    currentColor = Color.Black;
                    break;
            }
        }


        private void trbLineWidth_ValueChanged(object sender, EventArgs e)
        {
            currentLineWidth = trbLineWidth.Value;
        }

    }


}
