CREATE TABLE `websites` (
  `id` int(11) NOT NULL,
  `uuid` text NOT NULL,
  `user_token` text NOT NULL,
  `sftp_port` text NOT NULL,
  `mysql_port` text NOT NULL,
  `webserver_port` text NOT NULL,
  `webmanager_port` text NOT NULL,
  `webmanager_key` text NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

ALTER TABLE `websites`
  ADD PRIMARY KEY (`id`);

ALTER TABLE `websites`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
COMMIT;
