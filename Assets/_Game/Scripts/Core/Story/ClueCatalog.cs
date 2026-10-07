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
                case "c4_lich_tu_ba": title = "Nhịp đi lại của Tú Bà"; body = "Mụ đi giữa sảnh tiếp khách và phòng riêng. Mỗi lần về phòng mụ ngồi lâu hơn — rượu ngấm, rồi mụ sẽ ngủ gật trên ghế."; return true;
                case "c4_chia_khoa": title = "Chùm chìa đồng"; body = "Năm chiếc chìa lấy từ thắt lưng Tú Bà. Chiếc khắc hình con cá mở tủ khóa đồng; một chiếc vừa ổ khóa móc cửa sau."; return true;
                case "c4_giao_keo": title = "Giao kèo Mã Giám Sinh – Tú Bà"; body = "Lập từ tháng Giêng: Mã lo việc \"hỏi vợ\" ở các huyện, Tú Bà trả mỗi người bốn trăm năm mươi lạng. Ba cái tên trước Kiều đã bị gạch."; return true;
                case "c4_bac_dau": title = "Bạc đóng dấu \"T.\""; body = "Túi bạc bốn trăm năm mươi lạng lấy từ tủ của Tú Bà, mỗi nén có dấu đỏ của lầu Lâm Tri. Là bằng chứng — nhưng cũng dễ bị gọi là của trộm."; return true;
                case "c4_trang_no": title = "Trang sổ nợ của Kiều"; body = "\"Nợ bốn trăm năm mươi lạng tiền mua. Áo lụa: ba mươi lạng.\" Mới đến một ngày đã nợ một gia tài."; return true;
                case "c4_cua_sau": title = "Cửa sau nhà bếp"; body = "Cửa gỗ thấp cài then, khóa móc. Bên ngoài là sân sau có chó xích và người canh cầm đèn lồng."; return true;
                case "c4_co_lam": title = "Cô gái áo lam"; body = "Chị ấy đã ở lầu này ba năm và biết rõ từng lối đi. Chị ấy muốn thoát ra."; return true;
                case "c6_ba_hang_nuoc": title = "Nhân chứng: bà hàng nước"; body = "Bà nghe tận tai Mã Giám Sinh và tùy tùng nói chuyện \"giao hàng\" cho Tú Bà sau vách quán. Bà sẽ nói trước quan."; return true;
                case "c6_so_hon_thu": title = "Sổ hôn thú của huyện"; body = "Thư lại xác nhận Mã Giám Sinh vào sổ hôn thú bốn lần trong một năm, và bằng \"giám sinh\" của hắn là bằng mua."; return true;
                case "c6_so_hieu_bac": title = "Sổ hiệu bạc Phúc Ký"; body = "Ngày mười bốn tháng Tư, người nhà Tú Bà lấy bốn trăm năm mươi lạng giao cho \"Mã công tử\", người nhận điểm chỉ."; return true;
                case "c6_co_lam": title = "Nhân chứng: chị Lam"; body = "Ba năm trong lầu Tú Bà, một món nợ không bao giờ trả hết. Chị sẽ kể lại trước huyện đường."; return true;
                default: title = id; body = string.Empty; return false;
            }
        }
    }
}
