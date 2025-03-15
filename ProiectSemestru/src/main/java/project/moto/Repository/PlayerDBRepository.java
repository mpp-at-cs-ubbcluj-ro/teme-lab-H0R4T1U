package project.moto.Repository;

import project.moto.Domain.Player;
import project.moto.Utils.JdbcUtils;

import java.sql.Connection;
import java.util.ArrayList;
import java.util.Optional;
import java.util.Properties;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;

public class PlayerDBRepository implements Repository<Integer, Player>
{
    private final JdbcUtils dbUtils;
    private static final Logger logger = LogManager.getLogger();

    public PlayerDBRepository(Properties props) {
        this.dbUtils = new JdbcUtils(props);
        logger.info("Initialized PlayerDBIRepository with properties: {}", props);
    }

    @Override
    public Optional<Player> findOne(Integer aInt) {
        logger.info("Finding Player by ID: " + aInt);
        Connection con = dbUtils.getConnection();
        try (var preStmt = con.prepareStatement("select * from Player where id = ?")) {
            preStmt.setInt(1, aInt);
            try (var result = preStmt.executeQuery()) {
                if (result.next()) {
                    int id = result.getInt("Id");
                    String name = result.getString("Name");
                    String code = result.getString("Code");
                    Integer team = result.getInt("Team");
                    Player player = new Player(name, code, team);
                    player.setId(id);
                    logger.traceExit();
                    return Optional.of(player);
                }
            }
        } catch (Exception e) {
            logger.error(e);
            throw new RuntimeException(e);
        }
        logger.traceExit();
        return Optional.empty();
    }

    @Override
    public Iterable<Player> findAll() {
        logger.info("Finding all Players");
        Connection con = dbUtils.getConnection();
        ArrayList<Player> players = new ArrayList<>();
        try (var preStmt = con.prepareStatement("select * from Player")) {
            try (var result = preStmt.executeQuery()) {
                while (result.next()) {
                    int id = result.getInt("Id");
                    String name = result.getString("Name");
                    String code = result.getString("Code");
                    Integer team = result.getInt("Team");
                    Player player = new Player(name, code, team);
                    player.setId(id);
                    players.add(player);
                }
            }
        } catch (Exception e) {
            logger.error(e);
            throw new RuntimeException(e);
        }
        logger.traceExit(players.size());
        return players;
    }

    @Override
    public Optional<Player> save(Player entity) {
        logger.info("Saving Player: " + entity.getId());
        Connection con = dbUtils.getConnection();
        try (var preStmt = con.prepareStatement("insert into Player (Id,Name, Code, Team) values (?,?, ?, ?)")) {
            preStmt.setInt(1, entity.getId());
            preStmt.setString(2, entity.getName());
            preStmt.setString(3, entity.getCode());
            preStmt.setInt(4, entity.getTeam());
            preStmt.executeUpdate();
            logger.traceExit();
            return Optional.empty();
        } catch (Exception e) {
            logger.error(e);
            throw new RuntimeException(e);
        }
    }

    @Override
    public Optional<Player> delete(Integer aInt) {
        logger.info("Deleting Player: " + aInt);
        Connection con = dbUtils.getConnection();
        try (var preStmt = con.prepareStatement("delete from Player where id = ?")) {
            preStmt.setInt(1, aInt);
            preStmt.executeUpdate();
            logger.traceExit();
            return Optional.empty();
        } catch (Exception e) {
            logger.error(e);
            throw new RuntimeException(e);
        }
    }

    @Override
    public Optional<Player> update(Player entity) {
        logger.info("Updating Player: " + entity.getId());
        Connection con = dbUtils.getConnection();
        try (var preStmt = con.prepareStatement("update Player set Name = ?, Code = ?, Team = ? where id = ?")) {
            preStmt.setString(1, entity.getName());
            preStmt.setString(2, entity.getCode());
            preStmt.setInt(3, entity.getTeam());
            preStmt.setInt(4, entity.getId());
            preStmt.executeUpdate();
            logger.traceExit();
            return Optional.empty();
        } catch (Exception e) {
            logger.error(e);
            throw new RuntimeException(e);
        }

    }
}
