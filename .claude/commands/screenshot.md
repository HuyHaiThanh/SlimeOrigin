Chụp screenshot từ Game View để verify visual. Thực hiện:

1. Dùng `manage_camera` action="screenshot" include_image=True max_resolution=512
2. Nếu không có camera trong scene, dùng capture_source="scene_view" thay thế
3. Hiển thị ảnh và nhận xét ngắn: thấy gì, layout có đúng không, có gì bất thường không

Nếu $ARGUMENTS có tên object cụ thể, dùng view_target="$ARGUMENTS" để frame vào object đó.
