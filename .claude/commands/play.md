Toggle Unity Play Mode. Thực hiện:

1. Đọc `mcpforunity://editor/state` — check is_playing và advice.ready_for_tools
2. Nếu ready_for_tools=false → báo lý do và dừng lại, không toggle
3. Nếu is_playing=false → dùng `manage_editor` action="enter_play_mode"
4. Nếu is_playing=true → dùng `manage_editor` action="exit_play_mode"
5. Đọc lại editor/state sau 1 giây để confirm trạng thái mới
6. Báo cáo: đã chuyển sang [Play/Stop] mode
