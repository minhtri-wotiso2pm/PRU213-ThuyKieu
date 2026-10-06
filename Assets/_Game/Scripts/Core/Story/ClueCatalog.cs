namespace ThuyKieu.Core.Story
{
    /// <summary>Journal text for the "# clue:&lt;id&gt;" tags in Ink.</summary>
    public static class ClueCatalog
    {
        public static bool TryGet(string id, out string title, out string body)
        {
            switch (id)
            {
                case "c2_ban_tay": title = "Bàn tay đếm bạc"; body = "Mã Giám Sinh đếm bạc như người buôn tính lãi. Hắn và tùy tùng nhìn Kiều như nhìn một món hàng."; return true;
                case "c2_lam_tri": title = "Điểm đến: Lâm Tri"; body = "Hắn nói sẽ đưa Kiều về Lâm Tri, nơi \"có người lo liệu\". Không ai nhắc đến cha mẹ chồng hay lễ cưới."; return true;
                case "c2_mau_giay": title = "Mẩu giấy giao hàng"; body = "\"Bà T. ở Lâm Tri nhận hàng — giá bốn trăm năm mươi lạng.\" Hắn mua bốn trăm, định bán lại bốn trăm năm mươi."; return true;
                case "c2_tu_ba": title = "\"Tú Bà đã chuẩn bị chưa?\""; body = "Nghe lén sau quán nước: bên Lâm Tri đã dọn phòng, chỉ chờ giao người là trao bạc. Người nhận là Tú Bà."; return true;
                case "c3_co_hong": title = "Lời cô gái áo hồng"; body = "Cô ở đây ba năm. Lúc đến cũng có người hứa cưới. Muốn ra phố phải trả hết nợ."; return true;
                case "c3_co_lam": title = "Lời cô gái áo lam"; body = "Có người từng trèo tường trốn và bị bắt lại. Ngõ sau có chó, cổng trước có then."; return true;
                case "c3_cong_khoa": title = "Cổng khóa từ bên ngoài"; body = "Cổng chính khóa bằng ổ đồng từ phía ngoài. Người canh nói cổng chỉ mở cho khách."; return true;
                case "c3_nghe_len": title = "Cuộc mặc cả"; body = "Mã Giám Sinh đòi Tú Bà bốn trăm năm mươi lạng. Văn tự bán thân đã điểm chỉ, cất trong hộp sơn son. Chìa tủ ở thắt lưng Tú Bà."; return true;
                case "c3_so_no": title = "Sổ nợ son phấn"; body = "Mỗi cô gái có một trang nợ: son phấn, áo lụa, thuốc thang. Không trang nào ghi \"đã trả\"."; return true;
                case "c3_van_tu": title = "Văn tự bán thân"; body = "Người bán: Mã Giám Sinh. Người mua: Tú Bà. Người bị bán: Vương Thúy Kiều. Giá: bốn trăm năm mươi lạng."; return true;
                case "c3_tu_khoa": title = "Tủ khóa đồng"; body = "Trong phòng sổ sách có chiếc tủ gỗ gụ khóa ổ đồng. Thứ quý nhất của Tú Bà nằm trong đó."; return true;
                default: title = id; body = string.Empty; return false;
            }
        }
    }
}
