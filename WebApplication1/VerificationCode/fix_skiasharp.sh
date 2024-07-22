#���ݾ�Դ�ļ�
cp -rf /etc/apt/sources.list.d /etc/apt/sources.list.d.bak
#ɾ����Դ�ļ�
rm -rf /etc/apt/sources.list.d/*
#����Դ�ļ�ָ��Ϊ��Դ
cat <<'EOF'> /etc/apt/sources.list.d/huawei-cloud.list
deb https://mirrors.huaweicloud.com/debian/ bookworm main contrib non-free
deb https://mirrors.huaweicloud.com/debian/ bookworm-updates main contrib non-free
deb https://mirrors.huaweicloud.com/debian/ bookworm-backports main contrib non-free
deb https://mirrors.huaweicloud.com/debian-security/ bookworm-security main contrib non-free
EOF

cat <<'EOF'> fix_skiasharp_broke.sh
#!/bin/bash

# ���°���������ȷ������
apt-get update -y

# ��װ libgdiplus �⣬���� System.Drawing.Common �� Linux �ϵ�����
apt-get install -y libgdiplus

# ���� apt ���棬���پ����С
apt-get clean

# ����Ƿ���� /usr/lib/gdiplus.dll �ļ�����������ڣ��򴴽�һ��ָ�� libgdiplus.so �ķ�������
# ����Ϊ�˼���ĳЩ���� gdiplus.dll ��Ӧ��
if [ ! -f /usr/lib/gdiplus.dll ]; then
    ln -s /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll
fi
EOF

sh fix_skiasharp_broke.sh