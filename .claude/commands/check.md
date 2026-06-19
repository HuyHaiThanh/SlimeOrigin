Kiểm tra trạng thái Unity nhanh. Thực hiện tuần tự:

1. Đọc resource `mcpforunity://editor/state` — lấy: is_compiling, is_playing, active_scene, advice.ready_for_tools
2. Dùng `read_console` với types=["error","warning"], count=20, format="detailed"

Báo cáo ngắn gọn (dưới 10 dòng):
- Unity ready chưa? (ready_for_tools + is_compiling)
- Scene đang mở là gì?
- Có error nào không? (liệt kê nếu có)
- Có warning nào đáng chú ý không?
